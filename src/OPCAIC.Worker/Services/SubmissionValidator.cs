using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using OPCAIC.Messaging.Messages;
using OPCAIC.Worker.GameModules;

namespace OPCAIC.Worker.Services
{
	internal class SubmissionValidator
		: JobExecutorBase<SubmissionValidationRequest, SubmissionValidationResult>
	{
		/// <inheritdoc />
		public SubmissionValidator(ILogger<SubmissionValidator> logger, IExecutionServices services,
			IDownloadService downloadService) : base(logger, services, downloadService)
		{
		}

		private async Task RunSteps(CancellationToken cancellationToken)
		{
			Result.CheckerResult = await Invoke(nameof(IGameModule.Check), GameModule.Check,
				cancellationToken);
			if (Result.CheckerResult != SubTaskResult.Ok)
			{
				return;
			}

			Result.CompilerResult = await Invoke(nameof(IGameModule.Compile), GameModule.Compile,
				cancellationToken);
			if (Result.CompilerResult != SubTaskResult.Ok)
			{
				return;
			}

			Result.ValidatorResult = await Invoke(nameof(IGameModule.Validate), GameModule.Validate,
				cancellationToken);
			if (Result.ValidatorResult != SubTaskResult.Ok)
			{
				return;
			}

			Result.JobStatus = JobStatus.Ok;
		}

		/// <inheritdoc />
		protected override async Task DoUploadResults() 
			=> await DownloadService.UploadValidationResults(Request.ValidationId, OutputDirectory.FullName);

		/// <inheritdoc />
		protected override async Task InternalExecute(CancellationToken cancellationToken)
		{
			await DownloadSubmission(Request.SubmissionId);

			await RunSteps(cancellationToken);

			Logger.LogInformation("Submission validation completed.");
		}

		private async Task<SubTaskResult> Invoke<T>(
			string name, Func<SubmissionInfo, string, CancellationToken, Task<T>> entryPoint,
			CancellationToken cancellationToken) where T : GameModuleResult
		{
			using (Logger.EntryPointScope(name))
			{
				try
				{
					var result = await entryPoint(Submissions.Single(), OutputDirectory.FullName,
						cancellationToken);

					switch (result.EntryPointResult)
					{
						case GameModuleEntryPointResult.Success:
							Logger.LogInformation("{entrypoint} stage succeeded.", name);
							Result.JobStatus = JobStatus.Ok;
							return SubTaskResult.Ok;

						case GameModuleEntryPointResult.Failure:
							Logger.LogInformation("{entrypoint} stage failed.", name);
							Result.JobStatus = JobStatus.Ok; // still success
							return SubTaskResult.NotOk;

						case GameModuleEntryPointResult.ModuleError:
							Result.JobStatus = JobStatus.Error;
							Logger.LogInformation(
								"{entrypoint} stage invocation exited with an error {error}.",
								name,
								result.EntryPointResult);
							return SubTaskResult.ModuleError;

						default:
							throw new ArgumentOutOfRangeException();
					}
				}
				catch (Exception e) when (e is TaskCanceledException ||
					e is OperationCanceledException)
				{
					Logger.LogInformation("{entrypoint} stage was aborted.", name);
					Result.JobStatus = JobStatus.Timeout;
					return SubTaskResult.Aborted;
				}
				catch (GameModuleException e)
				{
					Logger.LogWarning(e,
						"Invocation of {entrypoint} entrypoint failed with exception.", name);
					Result.JobStatus = JobStatus.Error;
					return SubTaskResult.ModuleError;
				}
				catch (Exception e)
				{
					Logger.LogError(e, "Exception occured when invoking game module entry point.");
					Result.JobStatus = JobStatus.Error;
					return SubTaskResult.PlatformError;
				}
			}
		}
	}
}