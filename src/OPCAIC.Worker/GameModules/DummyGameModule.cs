using System.Threading;
using Chimera.Extensions.Logging.Log4Net;
using log4net;
using Microsoft.Extensions.Logging;

namespace OPCAIC.Worker.GameModules
{
	public class DummyGameModule : IGameModule
	{
		private readonly ILogger logger;

		public DummyGameModule(string gameName)
		{
			logger = new Log4NetLogger(LogManager.GetLogger(typeof(DummyGameModule)));
			GameName = gameName;
		}

		public string GameName { get; }

		public CheckerResult Check(string inputDir, string outputDir)
		{
			using (logger.BeginScope("Check"))
			{
				logger.LogInformation("Simulating work...");
				Thread.Sleep(100);
			}

			return null;
		}

		public CompilerResult Compile(string inputDir, string outputBinDir, string outputDir)
		{
			using (logger.BeginScope("Compile"))
			{
				logger.LogInformation("Simulating work...");
				Thread.Sleep(100);
			}

			return null;
		}

		public ValidatorResult Validate(string inputDir, string outputDir)
		{
			using (logger.BeginScope("Validate"))
			{
				logger.LogInformation("Simulating work...");
				Thread.Sleep(100);
			}

			return null;
		}

		public ExecutorResult Execute(string[] inputDirs, string outputDir)
		{
			using (logger.BeginScope("Execute"))
			{
				logger.LogInformation("Simulating work...");
				Thread.Sleep(100);
			}

			return null;
		}

		public void Clean()
		{
			using (logger.BeginScope("Clean"))
			{
				logger.LogInformation("Simulating work...");
				Thread.Sleep(100);
			}
		}
	}
}