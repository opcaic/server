using Microsoft.Extensions.Logging;
using OPCAIC.ApiService.Models.Submission;
using OPCAIC.Utils;

namespace OPCAIC.ApiService.Extensions
{
	internal static class SubmissionLoggingExtensions
	{
		public static void SubmissionCreated(this ILogger logger, long submissionId, NewSubmissionModel model)
		{
			logger.LogInformation(LoggingEvents.SubmissionCreate, $"Created new submission {{{LoggingTags.SubmissionId}}} to tournament {{{LoggingTags.TournamentId}}}", submissionId, model.TournamentId);
		}

		public static void SubmissionUpdated(this ILogger logger, long submissionId, UpdateSubmissionModel model)
		{
			logger.LogInformation(LoggingEvents.SubmissionUpdate, $"Updated submission {{{LoggingTags.SubmissionId}}}", submissionId);
		}
	}
}