namespace OPCAIC.Infrastructure.Dtos.Submissions
{
	public class SubmissionFilterDto : FilterDtoBase
	{
		public const string SortByCreated = "created";

		public long? AuthorId { get; set; }
		public bool? IsActive { get; set; }
		public long? TournamentId { get; set; }
		public long? MatchId { get; set; }
	}
}