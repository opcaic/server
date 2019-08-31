using System;
using System.IO;
using System.IO.Compression;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OPCAIC.GameModules.Interface;
using OPCAIC.Messaging.Messages;
using OPCAIC.Utils;
using OPCAIC.Worker.Config;
using OPCAIC.Worker.Exceptions;
using OPCAIC.Worker.GameModules;

namespace OPCAIC.Worker.Services
{
	internal class ExecutionServices : IExecutionServices
	{
		private readonly ExecutionConfig config;
		private readonly IGameModuleRegistry gameModuleRegistry;
		private readonly ILogger<ExecutionServices> logger;

		public ExecutionServices(ILogger<ExecutionServices> logger,
			IOptions<ExecutionConfig> config, IGameModuleRegistry gameModuleRegistry)
		{
			this.logger = logger;
			this.gameModuleRegistry = gameModuleRegistry;
			this.config = config.Value;
		}

		/// <inheritdoc />
		public DirectoryInfo GetWorkingDirectory(WorkMessageBase request)
		{
			return Directory.CreateDirectory(Path.Combine(config.WorkingDirectoryRoot,
				request.JobId.ToString()));
		}

		/// <inheritdoc />
		public void ArchiveDirectory(DirectoryInfo taskDirectory)
		{
			logger.LogTrace("Archiving task directory");
			Require.ArgNotNull(taskDirectory, nameof(taskDirectory));
			Require.That<ArgumentException>(taskDirectory.Exists, "Directory does not exist");

			// make sure archive exists
			Directory.CreateDirectory(config.ArchiveDirectoryRoot);

			ZipFile.CreateFromDirectory(taskDirectory.FullName,
				Path.Combine(config.ArchiveDirectoryRoot, taskDirectory.Name) + ".zip");
		}

		/// <inheritdoc />
		public IGameModule GetGameModule(string game)
		{
			return gameModuleRegistry.FindGameModule(game) ??
				throw new GameModuleNotFoundException(game);
		}
	}
}