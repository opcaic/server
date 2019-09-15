using OPCAIC.Domain.Enums;

namespace OPCAIC.Application.Dtos.Tournaments
{
	public class TournamentFilterDto : FilterDtoBase
	{
		public const string SortByName = "name";
		public const string SortByCreated = "created";

		public string Name { get; set; }

		public long? GameId { get; set; }

		public TournamentFormat? Format { get; set; }

		public TournamentScope? Scope { get; set; }

		public TournamentRankingStrategy? RankingStrategy { get; set; }
	}
}