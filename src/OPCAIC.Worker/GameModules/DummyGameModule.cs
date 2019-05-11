using System.Threading;
using Microsoft.Extensions.Logging;

namespace OPCAIC.Worker.GameModules
{
	class DummyGameModule : IGameModule
	{
		private ILogger logger;

		public DummyGameModule(ILogger<DummyGameModule> logger, string gameName)
		{
			this.logger = logger;
			GameName = gameName;
		}

		public string GameName { get; }

		public void Check(string inputDir, string outputDir)
		{
			using (logger.BeginScope("Check"))
			{
				logger.LogInformation("Simulating work...");
				Thread.Sleep(100);
			}
		}

		public void Compile(string inputDir, string outputDir)
		{
			using (logger.BeginScope("Compile"))
			{
				logger.LogInformation("Simulating work...");
				Thread.Sleep(100);
			}
		}

		public void Validate(string inputDir, string outputDir)
		{
			using (logger.BeginScope("Validate"))
			{
				logger.LogInformation("Simulating work...");
				Thread.Sleep(100);
			}
		}

		public void Execute(string inputDir, string outputDir)
		{
			using (logger.BeginScope("Execute"))
			{
				logger.LogInformation("Simulating work...");
				Thread.Sleep(100);
			}
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