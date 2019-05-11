using Microsoft.Extensions.Logging;
using OPCAIC.Messaging;
using OPCAIC.Messaging.Messages;
using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;

namespace OPCAIC.Worker
{
	public class WorkerSetConfig
	{
		public string BrokerAddress { get; set; }
		public WorkerConfig[] Workers { get; set; }
	}
	public class WorkerConfig
	{
		public string Identity { get; set; }
		public HeartbeatConfig HeartbeatConfig { get; set; }
		public string[] Supportedgames { get; set; }
	}

	public class Worker : IDisposable
	{
		private WorkerConnector connector;
		private ILogger logger;

		private string Identity => connector.Identity;

		public Worker(WorkerConnector connector, ILogger<Worker> logger)
		{
			this.connector = connector;
			this.logger = logger;
		}

		public void Run(string[] games)
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

			Thread t = new Thread(() => connector.EnterConsumer());
			t.Start();
			logger.LogInformation($"[{Identity}] - Initiating connection");
			connector.SendMessage(new WorkerConnectMessage()
			{
				Capabilities = new WorkerCapabilities()
				{
					SupportedGames = games.ToList(),
					SupportedLanguages = { "dotnet", "cpp" }
				},
			});
			connector.EnterPoller(); // returns on worker exit
			connector.StopConsumer();
			t.Join();
			logger.LogInformation($"[{Identity}] - client officially dead, restarting in 5s");
		}

		public void Dispose() => connector?.Dispose();
	}
}