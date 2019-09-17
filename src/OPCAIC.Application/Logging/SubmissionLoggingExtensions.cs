using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using OPCAIC.Application.Dtos.Submissions;
using OPCAIC.Common;

namespace OPCAIC.Application.Logging
{
	public static class SubmissionLoggingExtensions
	{
		public static void SubmissionCreated(this ILogger logger, long submissionId,
			NewSubmissionDto dto)
		{
			logger.LogInformation(LoggingEvents.SubmissionCreate,
				$"Created new submission {{{LoggingTags.SubmissionId}}} to tournament {{{LoggingTags.TournamentId}}}",
				submissionId, dto.TournamentId);
		}

		public static void SubmissionUpdated(this ILogger logger, long submissionId,
			UpdateSubmissionScoreDto dto)
		{
			logger.LogInformation(LoggingEvents.SubmissionUpdate,
				$"Updated submission {{{LoggingTags.SubmissionId}}}: {{{LoggingTags.UpdateData}}}",
				submissionId, JsonConvert.SerializeObject(dto));
		}
	}
}