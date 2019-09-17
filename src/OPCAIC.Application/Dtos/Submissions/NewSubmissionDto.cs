namespace OPCAIC.Application.Dtos.Submissions
{
	public class NewSubmissionDto
	{
		public long AuthorId { get; set; }
		public long TournamentId { get; set; }
		public long Score { get; set; }
	}
}