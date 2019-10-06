using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using OPCAIC.Application.Dtos.Tournaments;
using OPCAIC.Application.Tournaments.Commands;
using OPCAIC.Common;
using OPCAIC.Domain.Enums;

namespace OPCAIC.Application.Logging
{
	public static class TournamentLoggingExtensions
	{
		public static void TournamentCreated(this ILogger logger, long tournamentId,
			CreateTournamentCommand command)
		{
			logger.LogInformation(LoggingEvents.TournamentCreated,
				$"New tournament '{command.Name}' for game with id {{{LoggingTags.Game}}} was created with id {{{LoggingTags.TournamentId}}}",
				command.GameId, tournamentId);
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

		public static void TournamentUpdated<TDto>(this ILogger logger, long tournamentId,
			TDto dto)
		{
			logger.LogInformation(LoggingEvents.TournamentUpdated,
				$"Tournament {{{LoggingTags.TournamentId}}} was updated: {{@{LoggingTags.UpdateData}}}",
				tournamentId, dto);
		}
	}
}