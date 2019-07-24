using System;
using System.IO;
using System.IO.Compression;
using System.Threading.Tasks;
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
		private readonly ExecutionConfig config;
		private readonly IDownloadService downloadService;
		private readonly IGameModuleRegistry gameModuleRegistry;

		public ExecutionServices(IOptions<ExecutionConfig> config,
			IGameModuleRegistry gameModuleRegistry, IDownloadService downloadService)
		{
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
		public async Task DownloadSubmission(string serverPath, string localPath)
		{
			Require.ArgNotNull(serverPath, nameof(serverPath));
			Require.ArgNotNull(localPath, nameof(localPath));

			var bytes = await downloadService.DownloadBinaryAsync(serverPath);

			using (var archive = new ZipArchive(new MemoryStream(bytes), ZipArchiveMode.Read))
			{
				archive.ExtractToDirectory(localPath);
			}
		}

		/// <inheritdoc />
		public async Task UploadResults(WorkMessageBase request, DirectoryInfo outputDirectory)
		{
			Require.ArgNotNull(outputDirectory, nameof(outputDirectory));
			Require.That<ArgumentException>(outputDirectory.Exists, "Directory does not exist");

			var stream = new MemoryStream();
			using (var archive = new ZipArchive(stream, ZipArchiveMode.Create))
			{
				foreach (var fileInfo in outputDirectory.GetFiles("*", SearchOption.AllDirectories))
				{
					archive.CreateEntryFromFile(
						fileInfo.FullName,
						Path.GetRelativePath(outputDirectory.FullName, fileInfo.FullName));
				}

				// TODO: address
				await downloadService.UploadBinaryAsync($"results/{request.Id}", stream.ToArray());
			}
		}

		/// <inheritdoc />
		public void ArchiveDirectory(DirectoryInfo taskDirectory)
		{
			Require.ArgNotNull(taskDirectory, nameof(taskDirectory));
			Require.That<ArgumentException>(taskDirectory.Exists, "Directory does not exist");

			// make sure archive exists
			Directory.CreateDirectory(config.ArchiveDirectoryRoot);

			ZipFile.CreateFromDirectory(taskDirectory.FullName, Path.Combine(config.ArchiveDirectoryRoot, taskDirectory.Name) + ".zip");
		}
	}
}
