using OPCAIC.Infrastructure.Dtos.Matches;

namespace OPCAIC.Infrastructure.Dtos.Submissions
{
	public class SubmissionMatchResultDto
	{
		public SubmissionReferenceDto Submission { get; set; }
		public double Score { get; set; }
		public string AdditionalDataJson { get; set; }
	}
}