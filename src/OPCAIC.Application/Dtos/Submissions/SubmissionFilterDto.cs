using OPCAIC.Domain.Enums;

namespace OPCAIC.Application.Dtos.Submissions
{
	public class SubmissionFilterDto : FilterDtoBase
	{
		public const string SortByCreated = "created";
		public const string SortByAuthor = "author";

		public long? AuthorId { get; set; }
		public bool? IsActive { get; set; }
		public long? TournamentId { get; set; }
		public long? MatchId { get; set; }
		public string Author { get; set; }
		public SubmissionValidationState? ValidationState { get; set; }
	}
}