using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using Newtonsoft.Json;
using OPCAIC.Utils;
using OPCAIC.Worker.Config;

namespace OPCAIC.Worker.Services
{
	public interface IFileDownloadService
	{
		Task DownloadFile(string serverPath, string localPath);
		Task UploadFile(string serverPath, string localPath);
	}

	class FileDownloadService : IFileDownloadService, IDisposable
	{
		private readonly FileServerConfig config;
		private readonly WebClient webClient;

		public FileDownloadService(FileServerConfig config)
		{
			this.config = config;
			webClient = new WebClient();
			if (config.Username != null)
				webClient.Credentials = new NetworkCredential(config.Username, config.Password);
		}
		public Task DownloadFile(string serverPath, string localPath)
		{
			Require.NotNull(serverPath, nameof(serverPath));
			Require.NotNull(localPath, nameof(localPath));

			return webClient.DownloadFileTaskAsync(
				Path.Combine(config.ServerAddress, serverPath),
				localPath);
		}

		public Task UploadFile(string serverPath, string localPath)
		{
			Require.NotNull(serverPath, nameof(serverPath));
			Require.NotNull(localPath, nameof(localPath));

			return webClient.UploadFileTaskAsync(
				Path.Combine(config.ServerAddress, serverPath),
				localPath);
		}

		public void Dispose() => webClient?.Dispose();
	}
}
