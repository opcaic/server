using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using OPCAIC.GameModules.Interface;
using OPCAIC.Messaging.Messages;
using OPCAIC.Utils;
using OPCAIC.Worker.GameModules;
using BotInfo = OPCAIC.GameModules.Interface.BotInfo;

namespace OPCAIC.Worker.Services
{
	internal abstract class JobExecutorBase<TRequest, TResult> : IJobExecutor<TRequest, TResult>
		where TRequest : WorkMessageBase where TResult : ReplyMessageBase, new()
	{
		protected JobExecutorBase(ILogger logger, IExecutionServices services,
			IDownloadService downloadService, IGameModuleRegistry gameModuleRegistry)
		{
			Services = services;
			DownloadService = downloadService;
			GameModuleRegistry = gameModuleRegistry;
			Logger = logger;
			Response = new TResult();
			Submissions = new List<SubmissionData>();
			EntryPointConfig = new EntryPointConfiguration();
		}

		/// <summary>
		///     Configuration of the
		/// </summary>
		private EntryPointConfiguration EntryPointConfig { get; }

		/// <summary>
		///     Service for downloading/uploading files.
		/// </summary>
		protected IDownloadService DownloadService { get; }

		/// <summary>
		///     Various useful services.
		/// </summary>
		private IExecutionServices Services { get; }

		/// <summary>
		///     Registry of all game modules.
		/// </summary>
		private IGameModuleRegistry GameModuleRegistry { get; }

		/// <summary>
		///     Root directory for this task.
		/// </summary>
		private DirectoryInfo TaskDirectory { get; set; }

		/// <summary>
		///     Directory containing sources for individual submissions.
		/// </summary>
		private DirectoryInfo SourcesDirectory { get; set; }

		/// <summary>
		///     Directory containing compiled binaries for individual submissions.
		/// </summary>
		private DirectoryInfo BinariesDirectory { get; set; }

		/// <summary>
		///     Directory containing additional files for this task.
		/// </summary>
		private DirectoryInfo InputDirectory => EntryPointConfig.AdditionalFiles;

		/// <summary>
		///     Directory to save outputs items to be saved.
		/// </summary>
		protected DirectoryInfo OutputDirectory { get; private set; }

		/// <summary>
		///     Working data information about the submissions.
		/// </summary>
		protected List<SubmissionData> Submissions { get; }

		/// <summary>
		///     Response object of this task.
		/// </summary>
		protected TResult Response { get; }

		/// <summary>
		///     Relevant game module for this task.
		/// </summary>
		private IGameModule GameModule { get; set; }

		/// <summary>
		///     Logger for this task.
		/// </summary>
		protected ILogger Logger { get; }

		/// <summary>
		///     Original request object for this task.
		/// </summary>
		protected TRequest Request { get; private set; }

		/// <summary>
		///     The cancellation token associated with this job.
		/// </summary>
		protected CancellationToken CancellationToken { get; private set; }

		/// <inheritdoc />
		public async Task<TResult> ExecuteAsync(TRequest request,
			CancellationToken cancellationToken = new CancellationToken())
		{
			Require.ArgNotNull(request, nameof(request));
			using (CreateLoggingScope(request))
			{
				Request = request;
				Response.Id = request.Id;
				CancellationToken = cancellationToken;

				// create directory structure 
				GameModule = GameModuleRegistry.FindGameModule(request.Game);
				TaskDirectory = Services.GetWorkingDirectory(request);
				SourcesDirectory =
					TaskDirectory.CreateSubdirectory(Constants.DirectoryNames.Source);
				BinariesDirectory =
					TaskDirectory.CreateSubdirectory(Constants.DirectoryNames.Binary);
				OutputDirectory = TaskDirectory.CreateSubdirectory(Constants.DirectoryNames.Output);
				EntryPointConfig.AdditionalFiles =
					TaskDirectory.CreateSubdirectory(Constants.DirectoryNames.Input);

				try
				{
					await InternalExecute();
				}
				finally
				{
					await UploadResults();
					Services.ArchiveDirectory(TaskDirectory);
					TaskDirectory.Delete(true);
				}

				// If we got here, the evaluation of the job finished successfully.
				return Response;
			}
		}

		/// <summary>
		///     Creates a logging scope to be used for this request.
		/// </summary>
		/// <param name="request"></param>
		/// <returns></returns>
		protected abstract IDisposable CreateLoggingScope(TRequest request);

		protected abstract Task DoUploadResults();

		private async Task UploadResults()
		{
			try
			{
				if (!CancellationToken.IsCancellationRequested)
				{
					await DoUploadResults();
				}
			}
			catch (Exception e)
			{
				Logger.LogError(LoggingEvents.JobExecutionFailure, e, "Failed to upload results");
				// do not rethrow, there are other things that need to be done.
			}
		}

		protected async Task<SubmissionData> DownloadSubmission(long submissionId)
		{
			Logger.LogInformation($"Downloading submission {{{LoggingTags.SubmissionId}}}",
				submissionId);
			var index = Submissions.Count;
			var data = new SubmissionData
			{
				BotInfo = new BotInfo
				{
					Index = index,
					BinaryDirectory =
						BinariesDirectory.CreateSubdirectory(index.ToString()),
					SourceDirectory = SourcesDirectory.CreateSubdirectory(index.ToString())
				}
			};
			Submissions.Add(data);

			await DownloadService.DownloadSubmission(submissionId,
				data.BotInfo.SourceDirectory.FullName,
				CancellationToken);
			Require.That<InvalidOperationException>(
				data.BotInfo.SourceDirectory.Exists &&
				data.BotInfo.SourceDirectory.EnumerateFileSystemInfos().Any(),
				"No files found in the downloaded submission");

			return data;
		}

		protected abstract Task InternalExecute();

		protected async Task<SubTaskResult> Check(SubmissionData sub)
		{
			Logger.LogInformation($"Checking submission {{{LoggingTags.SubmissionId}}}.",
				sub.SubmissionId);
			return (await Invoke(nameof(IGameModule.Check), GameModule.Check, sub.BotInfo)).status;
		}

		protected async Task<SubTaskResult> Compile(SubmissionData sub)
		{
			Logger.LogInformation($"Compiling submission {{{LoggingTags.SubmissionId}}}.",
				sub.SubmissionId);
			return (await Invoke(nameof(IGameModule.Compile), GameModule.Compile, sub.BotInfo))
				.status;
		}

		protected async Task<SubTaskResult> Validate(SubmissionData sub)
		{
			Logger.LogInformation($"Validating submission {{{LoggingTags.SubmissionId}}}.",
				sub.SubmissionId);
			return (await Invoke(nameof(IGameModule.Validate), GameModule.Validate, sub.BotInfo))
				.status;
		}

		protected Task<(SubTaskResult status, ExecutorResult result)>
			Execute(IEnumerable<SubmissionData> subs)
			=> Invoke(nameof(IGameModule.Execute), GameModule.Execute, subs.Select(s => s.BotInfo));

		private async Task<(SubTaskResult status, T result)> Invoke<T, U>(string name,
			Func<EntryPointConfiguration, U, DirectoryInfo, CancellationToken, Task<T>> entryPoint,
			U botOrBots)
			where T : GameModuleResult
		{
			using (Logger.EntryPointScope(name))
			{
				T result = null;
				SubTaskResult status;
				try
				{
					result = await entryPoint(EntryPointConfig, botOrBots, OutputDirectory, CancellationToken);

					switch (result.EntryPointResult)
					{
						case GameModuleEntryPointResult.Success:
							status = SubTaskResult.Ok;
							break;

						case GameModuleEntryPointResult.Failure:
							status = SubTaskResult.NotOk;
							break;

						case GameModuleEntryPointResult.ModuleError:
							status = SubTaskResult.ModuleError;
							break;

						default:
							throw new ArgumentOutOfRangeException();
					}
				}
				catch (Exception e) when (e is TaskCanceledException ||
					e is OperationCanceledException)
				{
					Logger.LogWarning($"{{{LoggingTags.GameModuleEntryPoint}}} stage was aborted",
						name);
					status = SubTaskResult.Aborted;
				}
				catch (GameModuleException e)
				{
					Logger.LogError(LoggingEvents.GameModuleFailure, e,
						$"{{{LoggingTags.GameModuleEntryPoint}}} stage failed with exception ",
						name);
					status = SubTaskResult.ModuleError;
				}
				catch (Exception e)
				{
					Logger.LogCritical(LoggingEvents.GameModuleFailure, e,
						$"Invocation of {{{LoggingTags.GameModuleEntryPoint}}} stage failed with exception ",
						name);
					status = SubTaskResult.PlatformError;
				}

				return (status, result);
			}
		}

		/// <summary>
		///     Translates the (last) <see cref="SubTaskResult" /> status to appropriate <see cref="JobStatus" />.
		/// </summary>
		/// <param name="result">The last task result.</param>
		/// <param name="strict">Whether treat <see cref="SubTaskResult.NotOk" /> as an error.</param>
		/// <returns></returns>
		protected JobStatus SelectJobStatus(SubTaskResult result, bool strict = false)
		{
			if (strict && result == SubTaskResult.NotOk)
			{
				return JobStatus.Error;
			}

			switch (result)
			{
				case SubTaskResult.Ok:
				case SubTaskResult.NotOk: // still should exit successfully
					return JobStatus.Ok;
				case SubTaskResult.ModuleError:
				case SubTaskResult.PlatformError:
					return JobStatus.Error;
				case SubTaskResult.Aborted:
					return JobStatus.Timeout;
				default:
					throw new ArgumentOutOfRangeException(nameof(result), result, null);
			}
		}
	}
}