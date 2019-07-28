using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using OPCAIC.Messaging.Messages;

namespace OPCAIC.Worker.Services
{
	internal class MatchExecutor : JobExecutorBase<MatchExecutionRequest, MatchExecutionResult>
	{
		/// <inheritdoc />
		public MatchExecutor(ILogger<MatchExecutor> logger, IExecutionServices services,
			IDownloadService downloadService) : base(logger, services, downloadService)
		{
		}

		/// <inheritdoc />
		protected override Task DoUploadResults() => Task.CompletedTask;

		/// <inheritdoc />
		protected override Task InternalExecute(CancellationToken cancellationToken)
		{
			Result.JobStatus = JobStatus.Ok;
			Logger.LogInformation("Executing...");
			Thread.Sleep(1000);
			return Task.CompletedTask;
		}
	}
}