using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using OPCAIC.ApiService.Models.Submissions;
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
			// TODO: separate event for submission activation/deactivation
			logger.LogInformation(LoggingEvents.SubmissionUpdate, $"Updated submission {{{LoggingTags.SubmissionId}}}: {{{LoggingTags.UpdateData}}}", submissionId, JObject.FromObject(model));
		}
	}
}