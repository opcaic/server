using Microsoft.Extensions.Logging;

namespace OPCAIC.Worker.GameModules
{
	class ExternalGameModule : IGameModule
	{
		private readonly string rootDirectory;
		private readonly ILogger logger;

		public ExternalGameModule(string rootDirectory, string gameName, ILogger<ExternalGameModule> logger)
		{
			this.rootDirectory = rootDirectory;
			this.logger = logger;
			GameName = gameName;
		}

		public string GameName { get; }

		public void Check(string inputDir, string outputDir) => throw new System.NotImplementedException();

		public void Compile(string inputDir, string outputDir) => throw new System.NotImplementedException();

		public void Validate(string inputDir, string outputDir) => throw new System.NotImplementedException();

		public void Execute(string inputDir, string outputDir) => throw new System.NotImplementedException();

		public void Clean() => throw new System.NotImplementedException();
	}
}