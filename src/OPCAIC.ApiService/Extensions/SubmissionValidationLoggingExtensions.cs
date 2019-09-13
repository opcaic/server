using System;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using OPCAIC.Infrastructure.Dtos.SubmissionValidations;
using OPCAIC.Utils;

namespace OPCAIC.ApiService.Extensions
{
	internal static class SubmissionValidationLoggingExtensions
	{
		public static void SubmissionValidationQueued(this ILogger logger, long id,
			long submissionId, Guid jobId)
		{
			logger.LogInformation(LoggingEvents.SubmissionValidationQueued,
				$"Queuing validation for submission {{{LoggingTags.SubmissionId}}} as job {{{LoggingTags.JobId}}} with id {{{LoggingTags.SubmissionValidationId}}}",
				submissionId, jobId, id);
		}

		public static void SubmissionValidationUpdated(this ILogger logger, Guid jobId,
			UpdateSubmissionValidationDto dto)
		{
			logger.LogInformation(LoggingEvents.SubmissionValidationUpdated,
				$"Submission validation job {{{LoggingTags.JobId}}} updated: {{{LoggingTags.UpdateData}}}",
				jobId, JsonConvert.SerializeObject(dto));
		}

		public static void SubmissionValidationExpired(this ILogger logger, Guid jobId)
		{
			logger.LogWarning(LoggingEvents.SubmissionValidationUpdated,
				$"Submission validation job {{{LoggingTags.JobId}}} expired, returning back to queue",
				jobId);
		}

		public static void SubmissionValidationResultUploaded(this ILogger logger, long id)
		{
			logger.LogInformation(LoggingEvents.SubmissionValidationUploadResults,
				$"Uploaded results of Submission validation {{{LoggingTags.SubmissionValidationId}}}",
				id);
		}
	}
}