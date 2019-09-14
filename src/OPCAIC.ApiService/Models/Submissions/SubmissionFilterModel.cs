using OPCAIC.Infrastructure.Enums;

namespace OPCAIC.ApiService.Models.Submissions
{
	public class SubmissionFilterModel : FilterModelBase
	{
		public long? AuthorId { get; set; }
		public bool? IsActive { get; set; }
		public long? TournamentId { get; set; }
		public long? MatchId { get; set; }
		public string Author { get; set; }
		public SubmissionValidationState? ValidationState { get; set; }
	}
}