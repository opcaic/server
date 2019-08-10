namespace OPCAIC.Infrastructure.Dtos.Matches
{
	public class SubmissionMatchResultReferenceDto
	{
		public long SubmissionId { get; set; }
		public double Score { get; set; }
		public string AdditionalDataJson { get; set; }
	}
}