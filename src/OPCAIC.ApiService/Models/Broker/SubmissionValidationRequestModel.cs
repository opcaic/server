namespace OPCAIC.ApiService.Models.Broker
{
	public class SubmissionValidationRequestModel : WorkMessageBaseModel
	{
		public long SubmissionId { get; set; }

		public long ValidationId { get; set; }
	}
}