using Microsoft.Extensions.Logging;
using OPCAIC.Infrastructure.Enums;
using OPCAIC.Utils;

namespace OPCAIC.ApiService.Extensions
{
	internal static class TournamentLoggingExtensions
	{
		public static void TournamentStateChanged(this ILogger logger, long tournamentId,
			TournamentState state)
		{
			logger.LogInformation(LoggingEvents.TournamentStateChanged,
				$"Tournament {{{LoggingTags.TournamentId}}} is now in state {{{LoggingTags.TournamentState}}}.",
				tournamentId, state);
		}

		public static void TournamentMatchesGenerated(this ILogger logger, long tournamentId,
			int count)
		{
			logger.LogInformation(LoggingEvents.TournamentMatchesGeneration,
				$"Generated {{{LoggingTags.GeneratedMatchCount}}} matches for tournament {{{LoggingTags.TournamentId}}}",
				count, tournamentId);
		}
	}
}