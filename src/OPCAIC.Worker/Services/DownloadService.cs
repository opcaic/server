using System;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using OPCAIC.Utils;
using OPCAIC.Worker.Config;

namespace OPCAIC.Worker.Services
{
	internal class DownloadService : IDownloadService, IDisposable
	{
		private readonly WebClient webClient;

		public DownloadService(IOptions<FileServerConfig> config)
		{
			webClient = new WebClient();
			if (config.Value.UserName != null)
			{
				webClient.Credentials =
					new NetworkCredential(
						config.Value.UserName,
						config.Value.Password);
			}

			webClient.BaseAddress = config.Value.ServerAddress;
		}

		public void Dispose() => webClient?.Dispose();

		/// <inheritdoc />
		public Task DownloadAsync(string serverPath, string localPath)
		{
			Require.NotNull(serverPath, nameof(serverPath));
			Require.NotNull(localPath, nameof(localPath));

			return webClient.DownloadFileTaskAsync(
				serverPath,
				localPath);
		}

		/// <inheritdoc />
		public Task<byte[]> DownloadBinaryAsync(string serverPath)
		{
			Require.NotNull(serverPath, nameof(serverPath));

			return webClient.DownloadDataTaskAsync(serverPath);
		}

		/// <inheritdoc />
		public Task<string> DownloadTextAsync(string serverPath)
		{
			Require.NotNull(serverPath, nameof(serverPath));

			return webClient.DownloadStringTaskAsync(serverPath);
		}

		/// <inheritdoc />
		public async Task UploadAsync(string serverPath, string localPath, bool post)
		{
			Require.NotNull(serverPath, nameof(serverPath));
			Require.NotNull(localPath, nameof(localPath));

			var res = await webClient.UploadFileTaskAsync(
				serverPath,
				post ? "POST" : "PUT",
				localPath);

			Console.WriteLine(Encoding.ASCII.GetString(res));
		}

		/// <inheritdoc />
		public Task UploadBinaryAsync(string serverPath, byte[] data, bool post)
		{
			Require.NotNull(serverPath, nameof(serverPath));
			Require.NotNull(data, nameof(data));

			return webClient.UploadDataTaskAsync(
				serverPath,
				post ? "POST" : "PUT",
				data);
		}

		/// <inheritdoc />
		public Task UploadTextAsync(string serverPath, string data, bool post)
		{
			Require.NotNull(serverPath, nameof(serverPath));
			Require.NotNull(data, nameof(data));

			return webClient.UploadStringTaskAsync(
				serverPath,
				post ? "POST" : "PUT",
				data);
		}
	}
}
