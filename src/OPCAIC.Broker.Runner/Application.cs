using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OPCAIC.Messaging;
using OPCAIC.Messaging.Messages;
using OPCAIC.Worker;
using System;
using System.Collections.Generic;
using System.Threading;
namespace OPCAIC.Broker.Runner
{
	public class Application
	{
		private static readonly Random rand = new Random(42);
		private readonly Broker broker;
		private readonly ILogger logger;
		private readonly IServiceProvider serviceProvider;

		public Application(Broker broker, ILogger<Application> logger, IServiceProvider serviceProvider)
		{
			this.broker = broker;
			this.logger = logger;
			this.serviceProvider = serviceProvider;
		}

		private void StartWorkers()
		{
			var config = serviceProvider.GetRequiredService<WorkerSetConfig>();
			var hearbeat = serviceProvider.GetRequiredService<HeartbeatConfig>();
			foreach (var worker in config.Workers)
			{
				// bootstrap with custom configs
				var logger = serviceProvider.GetRequiredService<ILoggerFactory>();
				var services = new ServiceCollection()
					.AddSingleton(worker)
					.AddSingleton(new WorkerConnectorConfig
					{
						Identity = worker.Identity,
						BrokerAddress = config.BrokerAddress,
						HeartbeatConfig = hearbeat
					})
					.AddLogging(builder => builder.Services.AddSingleton<ILoggerFactory>(logger));

				Worker.Startup.ConfigureServices(services);

				Thread t = new Thread(() =>
				{
					while (true)
					{
						using (var workerApp = services.BuildServiceProvider().GetRequiredService<Worker.Worker>())
						{
							workerApp.Run();
						}
						Thread.Sleep(5000);
					}
				});

				t.Start();
			}
		}

		public void Run()
		{
			StartWorkers();
			RunBroker();
		}

		private void RunBroker()
		{
			List<MatchExecutionResultMessage> results = new List<MatchExecutionResultMessage>();

			var i = 0;
			broker.MatchExecuted += (_, a) =>
			{
				logger.LogInformation($"Finished: {a.Work}");
				results.Add(a);
			};
			broker.StartBrokering();
			var config = serviceProvider.GetRequiredService<BrokerConnectorConfig>();
			while (!Program.stop && i < 200)
			{
				Thread.Sleep(50);
				if (broker.GetUnfinishedTasksCount() > 20)
					continue;
				try
				{
					broker.EnqueueMatchExecution(new ExecuteMatchMessage
					{
						Game = config.Games[rand.Next(config.Games.Length)],
						Id = ++i
					});
				}
				catch (Exception e)
				{
					Console.WriteLine("Exception caught");
					Console.WriteLine(e);
				}
			}

			while (!Program.stop && results.Count < i)
			{
				Thread.Sleep(100);
			}

			Console.WriteLine($"Completed: {results.Count}/200 tasks");

			broker.StopBrokering();
		}
	}
}
