namespace OPCAIC.Application.Dtos.Submissions
{
	public class SubmissionAuthDto
	{
		public long Id { get; set; }
		public long AuthorId { get; set; }
		public long TournamentOwnerId { get; set; }
		public long[] TournamentManagersIds { get; set; }
	}
}