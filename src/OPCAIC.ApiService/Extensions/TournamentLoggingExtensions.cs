using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using OPCAIC.Infrastructure.Dtos.Tournaments;
using OPCAIC.Infrastructure.Enums;
using OPCAIC.Utils;

namespace OPCAIC.ApiService.Extensions
{
	internal static class TournamentLoggingExtensions
	{
		public static void TournamentCreated(this ILogger logger, long tournamentId,
			NewTournamentDto dto)
		{
			logger.LogInformation(LoggingEvents.TournamentCreated,
				$"New tournament '{dto.Name}' for game with id {{{LoggingTags.Game}}} was created with id {{{LoggingTags.TournamentId}}}",
				dto.GameId, tournamentId);
		}

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

		public static void TournamentUpdated(this ILogger logger, long tournamentId,
			UpdateTournamentDto dto)
		{
			logger.LogInformation(LoggingEvents.TournamentUpdated,
				$"Tournament {{{LoggingTags.TournamentId}}} was updated: {{{LoggingTags.UpdateData}}}",
				tournamentId, JsonConvert.SerializeObject(dto));
		}
	}
}