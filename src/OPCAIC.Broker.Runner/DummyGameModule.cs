using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using OPCAIC.Worker.GameModules;
using OPCAIC.Worker.Services;

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
		public async Task<CheckerResult> Check(SubmissionInfo submission, string outputDir,
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
		public async Task<CompilerResult> Compile(SubmissionInfo submission, string outputDir,
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
		public async Task<ValidatorResult> Validate(SubmissionInfo submission, string outputDir,
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
		public async Task<ExecutorResult> Execute(IEnumerable<SubmissionInfo> submissions,
			string outputDir, CancellationToken cancellationToken)
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