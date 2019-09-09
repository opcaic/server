using System;
using Microsoft.Extensions.Logging;
using OPCAIC.Utils;

namespace OPCAIC.ApiService.Extensions
{
	internal static class SubmissionValidationLoggingExtensions
	{
		public static void SubmissionValidationQueued(this ILogger logger, long submissionId, Guid jobId)
		{
			logger.LogInformation(LoggingEvents.SubmissionQueueValidation, $"Queuing validation for submission {{{LoggingTags.SubmissionId}}} as job {{{LoggingTags.JobId}}}", submissionId, jobId);
		}
	}
}