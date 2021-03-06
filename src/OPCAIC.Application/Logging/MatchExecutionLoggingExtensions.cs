﻿using System;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using OPCAIC.Application.Dtos.MatchExecutions;
using OPCAIC.Common;

namespace OPCAIC.Application.Logging
{
	public static class MatchExecutionLoggingExtensions
	{
		public static void MatchExecutionQueued(this ILogger logger, long id, long matchId,
			Guid jobId)
		{
			logger.LogInformation(LoggingEvents.MatchExecutionQueued,
				$"Queueing execution of match {{{LoggingTags.MatchId}}} as job {{{LoggingTags.JobId}}} with id {{{LoggingTags.ExecutionId}}}.",
				matchId, jobId, id);
		}

		public static void MatchExecutionUpdated(this ILogger logger, Guid jobId,
			object dto)
		{
			logger.LogInformation(LoggingEvents.MatchExecutionUpdated,
				$"Match execution job {{{LoggingTags.JobId}}} updated: {{@{LoggingTags.UpdateData}}}",
				jobId, dto);
		}

		public static void MatchExecutionExpired(this ILogger logger, Guid jobId)
		{
			logger.LogWarning(LoggingEvents.MatchExecutionUpdated,
				$"Match execution job {{{LoggingTags.JobId}}} expired, returning back to queue",
				jobId);
		}

		public static void MatchExecutionResultUploaded(this ILogger logger, long id)
		{
			logger.LogInformation(LoggingEvents.MatchExecutionUploadResults,
				$"Uploaded results of match execution {{{LoggingTags.ExecutionId}}}", id);
		}
	}
}