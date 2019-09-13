using System;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using OPCAIC.Utils;

namespace OPCAIC.Worker.Services
{
	internal class DownloadService : IDownloadService, IDisposable
	{
		private readonly HttpClient httpClient;
		private readonly AuthenticationHeaderValue authHeader;

		public DownloadService(HttpClient httpClient, AuthenticationHeaderValue authHeader)
		{
			this.httpClient = httpClient;
			this.authHeader = authHeader;
		}

		public void Dispose()
		{
			httpClient?.Dispose();
		}

		/// <inheritdoc />
		public Task DownloadSubmission(long submissionId, string path,
			CancellationToken cancellationToken = default)
		{
			Require.GreaterThan(submissionId, 0, nameof(submissionId));
			Require.ArgNotNull(path, nameof(path));

			return DownloadAndUnzip($"submissions/{submissionId}/download", path, cancellationToken);
		}

		/// <inheritdoc />
		public Task UploadValidationResults(long validationId, string path,
			CancellationToken cancellationToken = default)
		{
			Require.GreaterThan(validationId, 0, nameof(validationId));
			Require.ArgNotNull(path, nameof(path));

			return UploadFolderContents($"validation/{validationId}/result", path, cancellationToken);
		}

		/// <inheritdoc />
		public Task UploadMatchResults(long executionId, string path,
			CancellationToken cancellationToken = default)
		{
			Require.GreaterThan(executionId, 0, nameof(executionId));
			Require.ArgNotNull(path, nameof(path));

			return UploadFolderContents($"match-execution/{executionId}/result", path, cancellationToken);
		}

		/// <inheritdoc />
		public Task DownloadArchive(string url, string path, CancellationToken cancellationToken = default)
		{
			Require.ArgNotNull(url, nameof(url));
			Require.ArgNotNull(path, nameof(path));

			return DownloadAndUnzip(url, path, cancellationToken);
		}

		private async Task UploadFolderContents(string url, string path, CancellationToken cancellationToken)
		{
			var outputDirectory = new DirectoryInfo(path);
			Require.That<ArgumentException>(outputDirectory.Exists, "Directory does not exist");

			using (var stream = new MemoryStream())
			{
				using (var archive = new ZipArchive(stream, ZipArchiveMode.Create, true))
				{
					foreach (var fileInfo in outputDirectory.GetFiles("*", SearchOption.AllDirectories))
					{
						archive.CreateEntryFromFile(
							fileInfo.FullName,
							Path.GetRelativePath(outputDirectory.FullName, fileInfo.FullName));
					}
				}

				// rewind to beginning so that the data can be read form the stream
				stream.Seek(0, SeekOrigin.Begin);
				var content = new MultipartFormDataContent
				{
					{new StreamContent(stream), "archive", "archive.zip"}
				};

				await PostAsync(url, content, cancellationToken);
			}
		}

		private async Task DownloadAndUnzip(string url, string path, CancellationToken cancellationToken)
		{
			var stream = await GetStreamAsync(url, cancellationToken);
			using (var archive = new ZipArchive(stream, ZipArchiveMode.Read))
			{
				archive.ExtractToDirectory(path);
			}
		}

		private async Task<Stream> GetStreamAsync(string url, CancellationToken cancellationToken)
		{
			var httpRequestMessage = new HttpRequestMessage(HttpMethod.Get, url);
			httpRequestMessage.Headers.Authorization = authHeader;
			var response = await httpClient.SendAsync(httpRequestMessage, cancellationToken);
			response.EnsureSuccessStatusCode();
			var stream = response.Content != null
				? await response.Content.ReadAsStreamAsync()
				: Stream.Null;
			return stream;
		}

		private async Task PostAsync(string url, HttpContent content, CancellationToken cancellationToken)
		{
			var httpRequestMessage = new HttpRequestMessage(HttpMethod.Post, url) {Content = content};
			httpRequestMessage.Headers.Authorization = authHeader;
			var response = await httpClient.SendAsync(httpRequestMessage, cancellationToken);
			response.EnsureSuccessStatusCode();
		}
	}
}