using System.IO;
using System.IO.Compression;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using OPCAIC.Messaging.Messages;
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
			=> gameModuleRegistry.FindGameModule(game) ?? throw new GameModulueNotFoundException(game);

		/// <inheritdoc />
		public async Task DownloadSubmission(string serverPath, string localPath)
		{
			var bytes = await downloadService.DownloadBinaryAsync(serverPath);

			using (var archive = new ZipArchive(new MemoryStream(bytes), ZipArchiveMode.Read))
			{
				archive.ExtractToDirectory(localPath);
			}
		}
	}
}
