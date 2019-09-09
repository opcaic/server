using System;
using Microsoft.Extensions.Logging;
using OPCAIC.Utils;

namespace OPCAIC.ApiService.Extensions
{
	internal static class MatchExecutionLoggingExtensions
	{
		public static void MatchExecutionQueued(this ILogger logger, long matchId, Guid jobId)
		{
			logger.LogInformation(LoggingEvents.MatchQeueuExecution,
				$"Queueing execution of match {{{LoggingTags.MatchId}}} as job {{{LoggingTags.JobId}}}.",
				matchId, jobId);
		}
	}
}