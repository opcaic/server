using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using Microsoft.Extensions.Logging;
using OPCAIC.Messaging;
using OPCAIC.Messaging.Messages;
using OPCAIC.Worker.GameModules;

namespace OPCAIC.Worker
{
	public class Worker : IDisposable
	{
		private readonly WorkerConnector connector;
		private readonly ILogger logger;
		private readonly IGameModuleRegistry registry;

		Random rand = new Random();
		public Worker(WorkerConnector connector, ILogger<Worker> logger, IGameModuleRegistry registry)
		{
			this.connector = connector;
			this.logger = logger;
			this.registry = registry;

			connector.RegisterHandler<MatchExecutionRequest>(ExecuteMatch);
		}

		private string Identity => connector.Identity;

		public void Dispose() => connector?.Dispose();

		void ExecuteMatch(MatchExecutionRequest msg)
		{
			logger.LogInformation($"[{Identity}] - received workload: {msg.Game}");

			var module = registry.FindGameModule(msg.Game);
			Debug.Assert(module != null);

			module.Check(null, null);

			if (rand.Next(100) == 0)
			{
				logger.LogCritical($"[{Identity}] - simulating crash");
				connector.StopPoller();
			}

			connector.SendMessage(new MatchExecutionResult(msg.Id));
		}

		public void Run()
		{
			var t = new Thread(connector.EnterConsumer);
			t.Start();

			logger.LogInformation($"[{Identity}] - Initiating connection");
			connector.SendMessage(new WorkerConnectMessage
			{
				Capabilities = new WorkerCapabilities
				{
					SupportedGames = registry.GetAllModules().Select(m => m.GameName).ToList(),
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
