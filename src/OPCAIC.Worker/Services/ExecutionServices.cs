using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
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

			// create all directories
			Directory.CreateDirectory(config.Value.ArchiveDirectory);
			Directory.CreateDirectory(config.Value.ErrorDirectory);
			Directory.CreateDirectory(config.Value.WorkingDirectory);
		}

		/// <inheritdoc />
		public DirectoryInfo GetWorkingDirectory(string workIdentifier)
		{
			var path = Path.Combine(config.WorkingDirectory, workIdentifier);
			logger.LogTrace("Creating directory {path}", path);
			return Directory.CreateDirectory(path);
		}

		/// <inheritdoc />
		public void ArchiveDirectory(DirectoryInfo taskDirectory, bool success)
		{
			Require.ArgNotNull(taskDirectory, nameof(taskDirectory));
			Require.That<ArgumentException>(taskDirectory.Exists, "Directory does not exist");

			var targetDir = success
				? config.ArchiveDirectory
				: config.ErrorDirectory;


			// make sure target directory exists
			var dir = Directory.CreateDirectory(targetDir);

			var destinationArchiveFileName = Path.Combine(targetDir, taskDirectory.Name) + ".zip";

			logger.LogTrace($"Archiving task directory to {destinationArchiveFileName}");
			ZipFile.CreateFromDirectory(taskDirectory.FullName, destinationArchiveFileName);
		}

		/// <inheritdoc />
		public void DirectoryCleanup()
		{
			logger.LogInformation("Starting periodic cleanup of old directories.");
			DeleteOldFilesInDirectory(
				Directory.CreateDirectory(config.ArchiveDirectory),
				DateTime.Now.AddDays(-config.ArchiveRetentionDays));

			DeleteOldFilesInDirectory(
				Directory.CreateDirectory(config.ErrorDirectory),
				DateTime.Now.AddDays(-config.ErrorRetentionDays));
			logger.LogInformation("Directory cleanup finished.");
		}

		public void DeleteOldFilesInDirectory(DirectoryInfo directory, DateTime before)
		{
			var toDelete = directory.GetFileSystemInfos()
				.Where(i => i.CreationTime < before);

			var counter = 0;
			foreach (var file in toDelete)
			{
				counter++;
				file.Delete();
			}

			if (counter > 0)
			{
				logger.LogInformation($"Deleted {counter} items from {directory.FullName}");
			}
		}

		/// <inheritdoc />
		public IGameModule GetGameModule(string game)
		{
			return gameModuleRegistry.FindGameModule(game) ??
				throw new GameModuleNotFoundException(game);
		}
	}
}