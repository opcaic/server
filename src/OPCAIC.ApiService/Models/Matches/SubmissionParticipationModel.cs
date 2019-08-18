namespace OPCAIC.ApiService.Models.Matches
{
	public class SubmissionParticipationModel
	{
		public long MatchId { get; set; }
		public SubmissionReferenceModel Submission { get; set; }
	}
}