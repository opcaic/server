﻿using OPCAIC.Infrastructure.Enums;

namespace OPCAIC.Infrastructure.Dtos
{
	public class TournamentInfoDto
	{
		public long Id { get; set; }

		public string Name { get; set; }

		public string Description { get; set; }

		public long GameId { get; set; }

		public TournamentFormat Format { get; set; }

		public TournamentScope Scope { get; set; }

		public TournamentRankingStrategy RankingStrategy { get; set; }
	}
}