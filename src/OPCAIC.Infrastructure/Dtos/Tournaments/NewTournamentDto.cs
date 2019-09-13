using OPCAIC.Infrastructure.Enums;

namespace OPCAIC.Infrastructure.Dtos.Tournaments
{
	public class NewTournamentDto
	{
		public string Name { get; set; }

		public string Description { get; set; }

		public string Configuration { get; set; }

		public long GameId { get; set; }

		public TournamentFormat Format { get; set; }

		public TournamentScope Scope { get; set; }

		public TournamentRankingStrategy RankingStrategy { get; set; }

		public int? MatchesPerDay { get; set; }

		public long MaxSubmissionSize { get; set; }
	}
}