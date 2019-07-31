using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using OPCAIC.Messaging.Messages;
using OPCAIC.Utils;

namespace OPCAIC.Worker
{
	public static class ILoggingExtensions
	{
		public static IDisposable TaskScope<TMessage>(this ILogger logger, TMessage message)
			where TMessage : WorkMessageBase
			=> logger.BeginScope(new Dictionary<string, object>
			{
				[LoggingTags.JobId] = message.Id,
				[LoggingTags.Game] = message.Game,
				[LoggingTags.JobType] = typeof(TMessage)
			});

		public static IDisposable EntryPointScope(this ILogger logger, string name)
			=> logger.BeginScope(
				new Dictionary<string, object> {[LoggingTags.GameModuleEntryPoint] = name});

		public static IDisposable SubmissionValidationScope(this ILogger logger,
			SubmissionValidationRequest request)
			=> logger.BeginScope(new Dictionary<string, object>
			{
				[LoggingTags.SubmissionId] = request.SubmissionId,
				[LoggingTags.ValidationId] = request.ValidationId
			});

		public static IDisposable MatchExecutionScope(this ILogger logger,
			MatchExecutionRequest request)
			=> logger.BeginScope((LoggingTags.ExecutionId, request.ExecutionId));
	}
}