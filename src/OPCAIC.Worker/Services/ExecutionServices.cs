using System;
using System.IO;
using System.IO.Compression;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OPCAIC.Messaging.Messages;
using OPCAIC.Utils;
using OPCAIC.Worker.Config;
using OPCAIC.Worker.Exceptions;
using OPCAIC.Worker.GameModules;

namespace OPCAIC.Worker.Services
{
	internal class ExecutionServices : IExecutionServices
	{
		private readonly ILogger<ExecutionServices> logger;
		private readonly ExecutionConfig config;
		private readonly IDownloadService downloadService;
		private readonly IGameModuleRegistry gameModuleRegistry;

		public ExecutionServices(ILogger<ExecutionServices> logger,
			IOptions<ExecutionConfig> config, IGameModuleRegistry gameModuleRegistry, IDownloadService downloadService)
		{
			this.logger = logger;
			this.gameModuleRegistry = gameModuleRegistry;
			this.downloadService = downloadService;
			this.config = config.Value;
		}

		/// <inheritdoc />
		public DirectoryInfo GetWorkingDirectory(WorkMessageBase request)
			=> Directory.CreateDirectory(Path.Combine(config.WorkingDirectoryRoot,
				request.Id.ToString()));

		/// <inheritdoc />
		public IGameModule GetGameModule(string game)
			=> gameModuleRegistry.FindGameModule(game) ?? throw new GameModuleNotFoundException(game);

		/// <inheritdoc />
		public void ArchiveDirectory(DirectoryInfo taskDirectory)
		{
			logger.LogTrace("Archiving directory {directory}", taskDirectory);
			Require.ArgNotNull(taskDirectory, nameof(taskDirectory));
			Require.That<ArgumentException>(taskDirectory.Exists, "Directory does not exist");

			// make sure archive exists
			Directory.CreateDirectory(config.ArchiveDirectoryRoot);

			ZipFile.CreateFromDirectory(taskDirectory.FullName, Path.Combine(config.ArchiveDirectoryRoot, taskDirectory.Name) + ".zip");
		}
	}
}
