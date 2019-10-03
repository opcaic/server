using Microsoft.Extensions.Logging;
using OPCAIC.Application.Games.Commands;
using OPCAIC.Common;

namespace OPCAIC.Application.Logging
{
	public static class GameLoggingExtensions
	{
		public static void GameCreated(this ILogger logger, long id, CreateGameCommand dto)
		{
			logger.LogInformation(LoggingEvents.GameCreated,
				$"New game '{dto.Name}' was created with id {{{LoggingTags.GameId}}}", id);
		}

		public static void GameUpdated<TDto>(this ILogger logger, long id, TDto dto)
		{
			logger.LogInformation(LoggingEvents.GameUpdated,
				$"Game {{{LoggingTags.GameId}}} was updated: {{@{LoggingTags.UpdateData}}}", id,
				dto);
		}
	}
}