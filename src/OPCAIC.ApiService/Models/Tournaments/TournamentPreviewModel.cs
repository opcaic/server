using System;
using OPCAIC.Infrastructure.Enums;

namespace OPCAIC.ApiService.Models.Tournaments
{
	public class TournamentPreviewModel
	{
		public long Id { get; set; }

		public string Name { get; set; }

		public GameReferenceModel Game { get; set; }

		public TournamentFormat Format { get; set; }

		public TournamentScope Scope { get; set; }

		public TournamentRankingStrategy RankingStrategy { get; set; }

		public DateTime Created { get; set; }
	}
}