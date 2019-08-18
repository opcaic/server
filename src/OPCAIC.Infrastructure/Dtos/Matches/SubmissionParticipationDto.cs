namespace OPCAIC.Infrastructure.Dtos.Matches
{
	public class SubmissionParticipationDto
	{
		public long MatchId { get; set; }
		public SubmissionReferenceDto Submission { get; set; }
	}
}