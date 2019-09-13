using Newtonsoft.Json.Linq;
using OPCAIC.Infrastructure.Enums;

namespace OPCAIC.ApiService.Models.Tournaments
{
	public class NewTournamentModel
	{
		public string Name { get; set; }

		public string Description { get; set; }

		public long GameId { get; set; }

		public JObject Configuration { get; set; }

		public TournamentFormat Format { get; set; }

		public TournamentScope Scope { get; set; }

		public TournamentRankingStrategy RankingStrategy { get; set; }

		public int? MatchesPerDay { get; set; }
	}
}