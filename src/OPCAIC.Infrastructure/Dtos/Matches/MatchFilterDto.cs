namespace OPCAIC.Infrastructure.Dtos.Matches
{
	public class MatchFilterDto : FilterDtoBase
	{
		public const string SortByUpdated = "updated";
		public const string SortByCreated = "created";

		public long? TournamentId { get; set; }
		public long? UserId { get; set; }
		public bool? Executed { get; set; }
	}
}