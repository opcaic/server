using OPCAIC.Domain.Enums;

namespace OPCAIC.Infrastructure.Dtos.Matches
{
	public class MatchFilterDto : FilterDtoBase
	{
		public const string SortByUpdated = "updated";
		public const string SortByCreated = "created";

		public long? TournamentId { get; set; }
		public long? UserId { get; set; }
		public long? SubmissionId { get; set; }
		public MatchState? State { get; set; }
		public string Username { get; set; }
	}
}