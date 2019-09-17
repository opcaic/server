﻿using OPCAIC.Domain.Enums;

namespace OPCAIC.Application.Dtos.Games
{
	public class UpdateGameDto
	{
		public string Name { get; set; }

		public string Key { get; set; }

		public string ConfigurationSchema { get; set; }

		public string ImageUrl { get; set; }

		public string DefaultTournamentImageUrl { get; set; }

		public float? DefaultTournamentImageOverlay { get; set; }

		public string DefaultTournamentThemeColor { get; set; }

		public string Description { get; set; }

		public GameType Type { get; set; }

		public long MaxAdditionalFilesSize { get; set; }
	}
}