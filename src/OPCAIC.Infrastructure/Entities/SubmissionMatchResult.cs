namespace OPCAIC.Infrastructure.Entities
{
	public class SubmissionMatchResult : SoftDeletableEntity
	{
		public long SubmissionId { get; set; }
		public Submission Submission { get; set; }
		public double Score { get; set; }
		public string AdditionalData { get; set; }
	}
}