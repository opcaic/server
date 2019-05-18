using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OPCAIC.Messaging;
using OPCAIC.Messaging.Messages;
using OPCAIC.Worker;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using OPCAIC.Messaging.Config;
using OPCAIC.Worker.GameModules;

namespace OPCAIC.Broker.Runner
{
	public class Application
	{
		private static readonly Random rand = new Random(42);
		private readonly IBroker broker;
		private readonly ILogger logger;
		private readonly IServiceProvider serviceProvider;
		private readonly AppConfig config;

		public Application(IBroker broker, ILogger<Application> logger, IServiceProvider serviceProvider, IOptions<AppConfig> config)
		{
			this.broker = broker;
			this.logger = logger;
			this.serviceProvider = serviceProvider;
			this.config = config.Value;
		}

		private void StartWorkers()
		{
			foreach (var worker in config.WorkerSet.Workers ?? Enumerable.Empty<WorkerConfig>())
			{
				// bootstrap with custom configs
				var loggerFactory = serviceProvider.GetRequiredService<ILoggerFactory>();
				var services = new ServiceCollection()
					.AddSingleton(worker)
					.Configure<WorkerConnectorConfig>(cfg =>
					{
						cfg.Identity = worker.Identity;
						cfg.BrokerAddress = config.WorkerSet.BrokerAddress;
						cfg.HeartbeatConfig = config.Broker.HeartbeatConfig;
					})
					.AddLogging(builder => builder.Services.AddSingleton<ILoggerFactory>(loggerFactory));

				Worker.Startup.ConfigureServices(services);

				// replace with our custom module registry
				var registry = new GameModuleRegistry();
				foreach (var game in worker.Supportedgames)
					registry.AddModule(new DummyGameModule(game));
				services.RemoveAll(typeof(IGameModuleRegistry))
					.AddSingleton<IGameModuleRegistry>(registry);

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
			List<ReplyMessageBase> results = new List<ReplyMessageBase>();

			var i = 0;
			broker.RegisterHandler<MatchExecutionResult>(a =>
			{
				logger.LogInformation($"Finished: {a.Id}");
				results.Add(a);
			});

			broker.StartBrokering();
			while (!Program.Stop && i < 200)
			{
				Thread.Sleep(50);
				if (broker.GetUnfinishedTasksCount() > 20)
					continue;
				try
				{
					broker.EnqueueWork(new MatchExecutionRequest
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

			while (!Program.Stop && results.Count < i)
			{
				Thread.Sleep(100);
			}

			Console.WriteLine($"Completed: {results.Count}/200 tasks");

			broker.StopBrokering();
		}
	}
}
