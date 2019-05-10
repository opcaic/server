using Microsoft.Extensions.Logging;
using OPCAIC.Messaging;
using OPCAIC.Messaging.Messages;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;

namespace OPCAIC.Worker
{
	public class Worker : IDisposable
	{
		public static string ConnectionString = "tcp://localhost:5000"; // for test purposes

		private WorkerConnector connector;
		private ILogger logger;

		private string Identity => connector.Identity;

		public Worker(WorkerConnector connector, ILogger<Worker> logger)
		{
			this.connector = connector;
			this.logger = logger;
		}

		public void Run(List<string> games)
		{
			Random rand = new Random();
			connector.RegisterHandler<ExecuteMatchMessage>(msg =>
			{
				logger.LogInformation($"[{Identity}] - received workload: {msg.Game}");
				Debug.Assert(games.Contains(msg.Game));

				Thread.Sleep(500 * rand.Next(4));
				if (rand.Next(10) == 0)
				{
					logger.LogInformation($"[{Identity}] - simulating crash");
					connector.StopPoller();
				}

				connector.SendMessage(new MatchExecutionResultMessage(msg.Id));
			});

			while (true)
			{
				Thread t = new Thread(() => connector.EnterConsumer());
				t.Start();
				logger.LogInformation($"[{Identity}] - Initiating connection");
				connector.SendMessage(new WorkerConnectMessage()
				{
					Capabilities = new WorkerCapabilities()
					{
						SupportedGames = games,
						SupportedLanguages = { "dotnet", "cpp" }
					},
				});
				connector.EnterPoller(); // returns on worker exit
				connector.StopConsumer();
				t.Join();
				logger.LogInformation($"[{Identity}] - client officially dead, restarting in 5s");
				Thread.Sleep(5000);
			}
		}

		public void Dispose() => connector?.Dispose();
	}
}