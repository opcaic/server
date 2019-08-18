namespace OPCAIC.ApiService.Models.Matches
{
	public class SubmissionMatchResultModel
	{
		public long SubmissionId { get; set; }
		public double Score { get; set; }
		public string AdditionalDataJson { get; set; }
	}
}