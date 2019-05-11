using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using Microsoft.Extensions.Logging;
using OPCAIC.Messaging;
using OPCAIC.Messaging.Messages;

namespace OPCAIC.Worker
{
	public class Worker : IDisposable
	{
		private readonly WorkerConnector connector;
		private readonly ILogger logger;

		public Worker(WorkerConnector connector, ILogger<Worker> logger)
		{
			this.connector = connector;
			this.logger = logger;
		}

		private string Identity => connector.Identity;

		public void Dispose() => connector?.Dispose();

		public void Run(string[] games)
		{
			var rand = new Random();
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

			var t = new Thread(() => connector.EnterConsumer());
			t.Start();
			logger.LogInformation($"[{Identity}] - Initiating connection");
			connector.SendMessage(new WorkerConnectMessage
			{
				Capabilities = new WorkerCapabilities
				{
					SupportedGames = games.ToList(),
					SupportedLanguages = {"dotnet", "cpp"}
				}
			});
			connector.EnterPoller(); // returns on worker exit
			connector.StopConsumer();
			t.Join();
			logger.LogInformation($"[{Identity}] - client officially dead, restarting in 5s");
		}
	}
}
