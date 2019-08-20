using OPCAIC.Infrastructure.Dtos.Matches;

namespace OPCAIC.Infrastructure.Dtos.Submissions
{
	public class SubmissionParticipationDto
	{
		public MatchReferenceDto Match { get; set; }
		public SubmissionReferenceDto Submission { get; set; }
	}
}