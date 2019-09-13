using OPCAIC.Infrastructure.Enums;

namespace OPCAIC.ApiService.Models.Matches
{
	public class MatchFilterModel : FilterModelBase
	{
		public long? TournamentId { get; set; }
		public long? UserId { get; set; }

		public long? SubmissionId { get; set; }
		public MatchState? State { get; set; }
	}
}