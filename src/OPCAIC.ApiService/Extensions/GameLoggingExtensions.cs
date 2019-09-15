﻿using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using OPCAIC.Application.Dtos.Games;
using OPCAIC.Common;
using OPCAIC.Utils;

namespace OPCAIC.ApiService.Extensions
{
	internal static class GameLoggingExtensions
	{
		public static void GameCreated(this ILogger logger, long id, NewGameDto dto)
		{
			logger.LogInformation(LoggingEvents.GameCreated,
				$"New game '{dto.Name}' was created with id {{{LoggingTags.GameId}}}", id);
		}

		public static void GameUpdated(this ILogger logger, long id, UpdateGameDto dto)
		{
			logger.LogInformation(LoggingEvents.GameUpdated,
				$"Game {{{LoggingTags.GameId}}} was updated: {{{LoggingTags.UpdateData}}}", id,
				JsonConvert.SerializeObject(dto));
		}
	}
}