using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Threading;
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

		public void Dispose()
		{
			webClient?.Dispose();
		}

		/// <inheritdoc />
		public async Task DownloadSubmission(long submissionId, string path,
			CancellationToken cancellationToken = default)
		{
			Debug.Assert(submissionId > 0);
			Require.ArgNotNull(path, nameof(path));

			var bytes = await webClient.DownloadDataTaskAsync($"submissions/{submissionId}");

			using (var archive = new ZipArchive(new MemoryStream(bytes), ZipArchiveMode.Read))
			{
				archive.ExtractToDirectory(path);
			}
		}

		/// <inheritdoc />
		public Task UploadValidationResults(long validationId, string path,
			CancellationToken cancellationToken = default)
		{
			Require.ArgNotNull(path, nameof(path));
			var outputDirectory = new DirectoryInfo(path);
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
			}

			return webClient.UploadDataTaskAsync($"results/{validationId}", stream.ToArray());
		}

		/// <inheritdoc />
		public Task UploadMatchResults(long executionId, string path,
			CancellationToken cancellationToken = default)
		{
			throw new NotImplementedException();
		}
	}
}