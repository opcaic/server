using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OPCAIC.Worker.Config;
using OPCAIC.Worker.Services;

namespace OPCAIC.Broker.Runner
{
	/// <summary>
	///     Local download service which does not need a server endpoint.
	/// </summary>
	public class DummyDownloadService : IDownloadService
	{
		private readonly DirectoryInfo executionsDir;
		private readonly ILogger<DummyDownloadService> logger;
		private readonly DirectoryInfo storageDir;
		private readonly DirectoryInfo submissionsDir;
		private readonly DirectoryInfo validationsDir;

		public DummyDownloadService(ILogger<DummyDownloadService> logger,
			IOptions<FileServerConfig> config)
		{
			this.logger = logger;
			// HACK: reuse field for server address as storage path
			storageDir = new DirectoryInfo(config.Value.ServerAddress);
			submissionsDir = storageDir.CreateSubdirectory("submissions");
			validationsDir = storageDir.CreateSubdirectory("validations");
			executionsDir = storageDir.CreateSubdirectory("executions");
		}

		/// <inheritdoc />
		public Task DownloadSubmission(long submissionId, string path,
			CancellationToken cancellationToken = default)
		{
			logger.LogInformation($"Downloading submission {submissionId} to {path}");
			var submissionDir = submissionsDir.GetDirectories(submissionId.ToString())
				.SingleOrDefault();
			if (submissionDir == null)
			{
				throw new InvalidOperationException($"No submission with id {submissionId}");
			}

			CopyDirectory(submissionDir.FullName, path);

			return Task.CompletedTask;
		}

		/// <inheritdoc />
		public Task UploadValidationResults(long validationId, string path,
			CancellationToken cancellationToken = default)
		{
			logger.LogInformation($"Uploading validation results for {validationId} to {path}");

			return DoUpload(validationsDir, validationId, path);
		}

		/// <inheritdoc />
		public Task UploadMatchResults(long executionId, string path,
			CancellationToken cancellationToken = default)
		{
			logger.LogInformation($"Uploading match results for {executionId} to {path}");

			return DoUpload(executionsDir, executionId, path);
		}

		private Task DoUpload(DirectoryInfo target, long id, string source)
		{
			var destinationPath = Path.Combine(target.FullName, id.ToString());

			// make sure the directory is empty
			if (Directory.Exists(destinationPath))
			{
				Directory.Delete(destinationPath, true);
			}

			Directory.CreateDirectory(destinationPath);

			CopyDirectory(source, destinationPath);
			return Task.CompletedTask;
		}

		private void CopyDirectory(string sourcePath, string destinationPath)
		{
			Debug.Assert(Directory.Exists(destinationPath));
			Debug.Assert(!Directory.GetFiles(destinationPath).Any(),
				"destination path is not empty");

			// Create all of the directories
			foreach (var dirPath in Directory.GetDirectories(sourcePath, "*",
				SearchOption.AllDirectories))
			{
				Directory.CreateDirectory(dirPath.Replace(sourcePath, destinationPath));
			}

			// Copy all the files & Replaces any files with the same name
			foreach (var newPath in Directory.GetFiles(sourcePath, "*",
				SearchOption.AllDirectories))
			{
				File.Copy(newPath, newPath.Replace(sourcePath, destinationPath), true);
			}
		}
	}
}