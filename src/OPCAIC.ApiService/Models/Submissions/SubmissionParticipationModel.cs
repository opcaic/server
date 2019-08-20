namespace OPCAIC.ApiService.Models.Submissions
{
	public class SubmissionParticipationModel
	{
		public long MatchId { get; set; }
		public SubmissionReferenceModel Submission { get; set; }
	}
}