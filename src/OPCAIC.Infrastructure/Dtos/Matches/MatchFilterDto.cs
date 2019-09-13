using OPCAIC.Infrastructure.Enums;

namespace OPCAIC.Infrastructure.Dtos.Matches
{
	public class MatchFilterDto : FilterDtoBase
	{
		public const string SortByUpdated = "updated";
		public const string SortByCreated = "created";
		public const string SortByState = "state";

		public long? TournamentId { get; set; }
		public long? UserId { get; set; }
		public long? SubmissionId { get; set; }
		public MatchState? State { get; set; }
	}
}