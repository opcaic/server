using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using OPCAIC.Common;
using OPCAIC.Messaging.Messages;

namespace OPCAIC.Worker
{
	public static class ILoggingExtensions
	{
		public static IDisposable TaskScope<TMessage>(this ILogger logger, TMessage message)
			where TMessage : WorkMessageBase
		{
			return logger.BeginScope(new Dictionary<string, object>
			{
				[LoggingTags.JobId] = message.JobId,
				[LoggingTags.Game] = message.GameKey,
				[LoggingTags.TournamentId] = message.TournamentId,
				[LoggingTags.GameId] = message.GameId,
				[LoggingTags.JobType] = typeof(TMessage)
			});
		}

		public static IDisposable EntryPointScope(this ILogger logger, string name)
		{
			return logger.BeginScope(
				new Dictionary<string, object> {[LoggingTags.GameModuleEntryPoint] = name});
		}

		public static IDisposable SubmissionValidationScope(this ILogger logger,
			SubmissionValidationRequest request)
		{
			return logger.BeginScope(new Dictionary<string, object>
			{
				[LoggingTags.SubmissionId] = request.SubmissionId,
				[LoggingTags.ValidationId] = request.ValidationId
			});
		}

		public static IDisposable MatchExecutionScope(this ILogger logger,
			MatchExecutionRequest request)
		{
			return logger.BeginScope(new Dictionary<string, object>
			{
				[LoggingTags.ExecutionId] = request.ExecutionId,
				[LoggingTags.MatchId] = request.MatchId
			});
		}
	}
}