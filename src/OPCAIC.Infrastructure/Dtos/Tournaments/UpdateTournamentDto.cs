using System.ComponentModel.DataAnnotations;
using OPCAIC.Infrastructure.Enums;

namespace OPCAIC.Infrastructure.Dtos.Tournaments
{
	public class UpdateTournamentDto
	{
		[Required]
		[MinLength(1)]
		public string Name { get; set; }

		public string Description { get; set; }

		public long GameId { get; set; }

		public TournamentFormat Format { get; set; }

		public TournamentScope Scope { get; set; }

		public TournamentRankingStrategy RankingStrategy { get; set; }

		public string MenuData { get; set; }
	}
}