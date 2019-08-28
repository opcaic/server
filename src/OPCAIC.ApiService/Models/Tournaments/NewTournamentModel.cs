using Newtonsoft.Json.Linq;
using OPCAIC.ApiService.ModelValidationHandling.Attributes;
using OPCAIC.Infrastructure;
using OPCAIC.Infrastructure.Entities;
using OPCAIC.Infrastructure.Enums;

namespace OPCAIC.ApiService.Models.Tournaments
{
	public class NewTournamentModel
	{
		[ApiRequired]
		[ApiMaxLength(StringLengths.TournamentName)]
		public string Name { get; set; }

		public string Description { get; set; }

		[ApiEntityReference(typeof(Game))]
		public long GameId { get; set; }

		public JObject Configuration { get; set; }

		public TournamentFormat Format { get; set; }

		public TournamentScope Scope { get; set; }

		public TournamentRankingStrategy RankingStrategy { get; set; }
	}
}