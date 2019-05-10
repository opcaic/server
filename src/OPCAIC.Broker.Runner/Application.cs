using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OPCAIC.Messaging;
using OPCAIC.Messaging.Messages;
using OPCAIC.Worker;
using System;
using System.Threading;

namespace OPCAIC.Broker.Runner
{
	public class Application
	{
		private readonly Broker broker;
		private readonly ILogger logger;
		private readonly IServiceProvider serviceProvider;

		static readonly Random rand = new Random(42);

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
				Thread t = new Thread(() =>
				{
					while (true)
					{
						using (var connector = new WorkerConnector(config.BrokerAddress, worker.Identity, hearbeat))
						{
							var W = new Worker.Worker(connector,
								serviceProvider.GetRequiredService<ILogger<Worker.Worker>>());
							W.Run(worker.Supportedgames);
						}
					}
				});
				t.Start();
			}
		}

		public void Run()
		{
			StartWorkers();

			int i = 1;
			broker.MatchExecuted += (_, a) => logger.LogInformation($"Finished: {a.Work}");
			broker.StartBrokering();
			while (!Program.stop)
			{
				Thread.Sleep(1000);
				try
				{
					broker.EnqueueMatchExecution(new ExecuteMatchMessage()
					{
						Game = Shared.Games[rand.Next(Shared.Games.Count)],
						Id = i++
					});
				}
				catch (Exception e)
				{
					Console.WriteLine("Exception caught");
					Console.WriteLine(e);
				}
			}
			broker.StopBrokering();
		}
	}
}