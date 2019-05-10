using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OPCAIC.Messaging.Messages;
using OPCAIC.Worker;

namespace OPCAIC.Broker.Runner
{
	public class Application
	{
		private const int workerCount = 5;

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
			List<Thread> workers = new List<Thread>(workerCount);
			for (int i = 0; i < workerCount; i++)
			{
				var games = Shared.Games.Where(_ => rand.Next(4) > 0).ToList();
				workers.Add(new Thread(() =>
				{
					while (true)
					{
						using (var worker = serviceProvider.GetRequiredService<Worker.Worker>())
						{
							worker.Run(games);
						}
					}
				}));
				workers[i].Start();
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