using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using OPCAIC.GameModules.Interface;
using OPCAIC.Messaging.Messages;
using OPCAIC.Worker.GameModules;
using MessageBotInfo = OPCAIC.Messaging.Messages.BotInfo;
using PlatformBotResult = OPCAIC.GameModules.Interface.BotResult;
using MessageBotResult = OPCAIC.Messaging.Messages.BotResult;

namespace OPCAIC.Worker.Services
{
	internal class MatchExecutor : JobExecutorBase<MatchExecutionRequest, MatchExecutionResult>
	{
		/// <inheritdoc />
		public MatchExecutor(ILogger<MatchExecutor> logger, IExecutionServices services,
			IDownloadService downloadService, IGameModuleRegistry gameModuleRegistry) : base(logger,
			services, downloadService, gameModuleRegistry)
		{
		}

		/// <inheritdoc />
		protected override IDisposable CreateLoggingScope(MatchExecutionRequest request)
		{
			return Logger.MatchExecutionScope(request);
		}

		/// <inheritdoc />
		protected override Task DoUploadResults()
		{
			return DownloadService.UploadMatchResults(Request.ExecutionId, OutputDirectory.FullName,
				CancellationToken);
		}

		/// <inheritdoc />
		protected override async Task InternalExecute()
		{
			PrepareResponse();

			Response.JobStatus = SelectJobStatus(await PrepareAllSubmissions(), true);
			if (Response.JobStatus != JobStatus.Ok)
			{
				return;
			}

			Logger.LogInformation("Executing the match.");
			ExecutorResult result;
			(Response.ExecutionResult, result) = await Execute(Submissions);

			if (Response.ExecutionResult < SubTaskResult.Aborted)
			{
				Debug.Assert(result != null);

				var matchResult = result.MatchResult;

				// copy the results
				Response.AdditionalData = matchResult.AdditionalInfo;
				for (var i = 0; i < matchResult.Results.Length; i++)
				{
					var botResult = matchResult.Results[i];

					Response.BotResults[i].Score = botResult.Score;
					Response.BotResults[i].AdditionalData = botResult.AdditionalInfo;
					Response.BotResults[i].Crashed = botResult.HasCrashed;
				}
			}

			Response.JobStatus = SelectJobStatus(Response.ExecutionResult, true);
		}

		private void PrepareResponse()
		{
			Response.BotResults = new MessageBotResult[Request.Bots.Count];
			for (var i = 0; i < Response.BotResults.Length; i++)
			{
				Response.BotResults[i] = new MessageBotResult();
			}
		}

		private async Task<SubTaskResult> PrepareAllSubmissions()
		{
			Logger.LogInformation("Preparing submissions");
			// download all submissions and compile them
			var results = await Task.WhenAll(Request.Bots.Select(PrepareSubmission).ToList());

			// if cancellation requested, return such, otherwise return most severe error
			return results.Contains(SubTaskResult.Aborted)
				? SubTaskResult.Aborted
				: results.Max();
		}

		private async Task<SubTaskResult> PrepareSubmission(MessageBotInfo botInfo)
		{
			var sub = await DownloadSubmission(botInfo.SubmissionId);

			Logger.LogInformation("Compiling submission {sub}.", botInfo.SubmissionId);

			var status = await Compile(sub);

			Response.BotResults[sub.BotInfo.Index].CompilationResult = status;

			return status;
		}
	}
}