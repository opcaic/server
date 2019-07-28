using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using OPCAIC.GameModules.Interface;

namespace OPCAIC.Broker.Runner
{
	public class DummyGameModule : IGameModule
	{
		private const int workTime = 10;
		private readonly ILogger logger;

		public DummyGameModule(string gameName, ILogger logger)
		{
			GameName = gameName;
			this.logger = logger;
		}

		/// <inheritdoc />
		public string GameName { get; }

		/// <inheritdoc />
		public async Task<CheckerResult> Check(EntryPointConfiguration config, BotInfo bot,
			DirectoryInfo outputDir,
			CancellationToken cancellationToken)
		{
			using (logger.BeginScope("Check"))
			{
				logger.LogInformation("Simulating work...");
				Thread.Sleep(workTime);
			}

			return null;
		}

		/// <inheritdoc />
		public async Task<CompilerResult> Compile(EntryPointConfiguration config, BotInfo bot,
			DirectoryInfo outputDir,
			CancellationToken cancellationToken)
		{
			using (logger.BeginScope("Compile"))
			{
				logger.LogInformation("Simulating work...");
				Thread.Sleep(workTime);
			}

			return null;
		}


		/// <inheritdoc />
		public async Task<ValidatorResult> Validate(EntryPointConfiguration config, BotInfo bot,
			DirectoryInfo outputDir,
			CancellationToken cancellationToken)
		{
			using (logger.BeginScope("Validate"))
			{
				logger.LogInformation("Simulating work...");
				Thread.Sleep(workTime);
			}

			return null;
		}

		/// <inheritdoc />
		public async Task<ExecutorResult> Execute(EntryPointConfiguration config,
			IEnumerable<BotInfo> submissions,
			DirectoryInfo outputDir, CancellationToken cancellationToken)
		{
			using (logger.BeginScope("Execute"))
			{
				logger.LogInformation("Simulating work...");
				Thread.Sleep(workTime);
			}

			return null;
		}

		/// <inheritdoc />
		public async Task Clean(CancellationToken cancellationToken)
		{
			using (logger.BeginScope("Clean"))
			{
				logger.LogInformation("Simulating work...");
				Thread.Sleep(workTime);
			}
		}
	}
}