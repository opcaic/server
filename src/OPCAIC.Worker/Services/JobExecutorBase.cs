using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using OPCAIC.Messaging.Messages;
using OPCAIC.Utils;
using OPCAIC.Worker.GameModules;

namespace OPCAIC.Worker.Services
{
	internal abstract class JobExecutorBase<TRequest, TResult> : IJobExecutor<TRequest, TResult>
		where TRequest : WorkMessageBase where TResult : ReplyMessageBase, new()
	{
		protected JobExecutorBase(ILogger logger, IExecutionServices services)
		{
			Services = services;
			Logger = logger;
			Result = new TResult();
			Submissions = new List<SubmissionInfo>();
		}

		/// <summary>
		///     Various useful services.
		/// </summary>
		public IExecutionServices Services { get; }

		/// <summary>
		///     Root directory for this task.
		/// </summary>
		protected DirectoryInfo TaskDirectory { get; private set; }

		/// <summary>
		///     Directory containing sources for individual submissions.
		/// </summary>
		protected DirectoryInfo SourcesDirectory { get; private set; }

		/// <summary>
		///     Directory containing compiled binaries for individual submissions.
		/// </summary>
		protected DirectoryInfo BinariesDirectory { get; private set; }

		/// <summary>
		///     Directory containing additional files for this task.
		/// </summary>
		protected DirectoryInfo InputDirectory { get; private set; }

		/// <summary>
		///     Directory to save outputs items to be saved.
		/// </summary>
		protected DirectoryInfo OutputDirectory { get; private set; }

		/// <summary>
		///     Working data information about the submissions.
		/// </summary>
		protected List<SubmissionInfo> Submissions { get; set; }
		
		/// <summary>
		///     Response object of this task.
		/// </summary>
		protected TResult Result { get; }

		/// <summary>
		///     Relevant game module for this task.
		/// </summary>
		protected IGameModule GameModule { get; private set; }

		/// <summary>
		///     Logger for this task.
		/// </summary>
		protected ILogger Logger { get; }

		/// <summary>
		///     Original request object for this task.
		/// </summary>
		protected TRequest Request { get; set; }

		/// <inheritdoc />
		public async Task<TResult> ExecuteAsync(TRequest request,
			CancellationToken cancellationToken = new CancellationToken())
		{
			Require.ArgNotNull(request, nameof(request));

			Request = request;
			Result.Id = request.Id;

			// create directory structure 
			GameModule = Services.GetGameModule(request.Game);
			TaskDirectory = Services.GetWorkingDirectory(request);
			SourcesDirectory = TaskDirectory.CreateSubdirectory(Constants.DirectoryNames.Source);
			BinariesDirectory = TaskDirectory.CreateSubdirectory(Constants.DirectoryNames.Binary);
			InputDirectory = TaskDirectory.CreateSubdirectory(Constants.DirectoryNames.Input);
			OutputDirectory = TaskDirectory.CreateSubdirectory(Constants.DirectoryNames.Output);

			try
			{
				await InternalExecute(cancellationToken);
			}
			finally
			{
				// cleanup
//				await Services.UploadResults(request, OutputDirectory);
				Services.ArchiveDirectory(TaskDirectory);
				TaskDirectory.Delete(true);
			}

			// If we got here, the evaluation of the job finished successfully.
			return Result;
		}

		protected async Task<SubmissionInfo> DownloadSubmission(string serverPath)
		{
			Logger.LogInformation($"Downloading submission '{serverPath}'");
			var index = Submissions.Count;
			var info = new SubmissionInfo()
			{
				Index = index,
				BinaryDirectory = BinariesDirectory.CreateSubdirectory(index.ToString()),
				SourceDirectory = SourcesDirectory.CreateSubdirectory(index.ToString())
			};
			Submissions.Add(info);
            await Services.DownloadSubmission(serverPath, info.SourceDirectory.FullName);
			Require.That<InvalidOperationException>(
				info.SourceDirectory.Exists && info.SourceDirectory.EnumerateFileSystemInfos().Any(),
				"No files found in the downloaded submission");
            return info;
		}

		protected string PathTo(string path) => Path.Combine(TaskDirectory.FullName, path);

		protected abstract Task InternalExecute(CancellationToken cancellationToken);
	}
}
