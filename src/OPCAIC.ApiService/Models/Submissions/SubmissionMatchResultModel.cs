using OPCAIC.ApiService.Models.Matches;

namespace OPCAIC.ApiService.Models.Submissions
{
	public class SubmissionMatchResultModel
	{
		public SubmissionReferenceModel Submission { get; set; }
		public double Score { get; set; }
		public string AdditionalDataJson { get; set; }
	}
}