using System;
using OPCAIC.Infrastructure.Dtos.Games;
using OPCAIC.Infrastructure.Enums;

namespace OPCAIC.Infrastructure.Dtos.Tournaments
{
	public class TournamentPreviewDto
	{
		public long Id { get; set; }

		public string Name { get; set; }

		public GameReferenceDto Game { get; set; }

		public TournamentFormat Format { get; set; }

		public TournamentScope Scope { get; set; }

		public TournamentRankingStrategy RankingStrategy { get; set; }

		public DateTime Created { get; set; }

		public int PlayersCount { get; set; }

		public int SubmissionsCount { get; set; }

		public string ImageUrl { get; set; }

		public double? ImageOverlay { get; set; }

		public string ThemeColor { get; set; }

		public DateTime? Deadline { get; set; }

		public TournamentAvailability Availability { get; set; }
	}
}