using System;
using System.Linq;
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
			IDownloadServiceFactory downloadServiceFactory, IGameModuleRegistry registry) 
			: base(logger, services, downloadServiceFactory, registry)
		{
		}

		private async Task<SubTaskResult> RunSteps()
		{
			var sub = Submissions.Single();

			Response.CheckerResult = await Check(sub);
			if (Response.CheckerResult != SubTaskResult.Ok)
			{
				return Response.CheckerResult;
			}

			Response.CompilerResult = await Compile(sub);
			sub.CompilerResult = Response.CompilerResult;
			if (Response.CompilerResult != SubTaskResult.Ok)
			{
				return Response.CompilerResult;
			}

			Response.ValidatorResult = await Validate(sub);

			return Response.ValidatorResult;
		}

		/// <inheritdoc />
		protected override IDisposable CreateLoggingScope(SubmissionValidationRequest request)
		{
			return Logger.SubmissionValidationScope(request);
		}

		/// <inheritdoc />
		protected override async Task DoUploadResults()
		{
			await DownloadService.UploadValidationResults(Request.ValidationId,
				OutputDirectory.FullName, CancellationToken);
		}

		/// <inheritdoc />
		protected override async Task InternalExecute()
		{
			await DownloadSubmission(Request.SubmissionId);

			Response.JobStatus = SelectJobStatus(await RunSteps());

			Logger.LogInformation("Submission validation completed.");
		}
	}
}