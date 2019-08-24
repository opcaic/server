namespace OPCAIC.Infrastructure.Dtos.Broker
{
	public class SubmissionValidationRequestDto : WorkMessageBaseDto
	{
		public long SubmissionId { get; set; }

		public long ValidationId { get; set; }
	}
}