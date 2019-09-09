namespace OPCAIC.ApiService.Models.SubmissionValidations
{
	public class SubmissionValidationDetailModel : SubmissionValidationPreviewModel
	{
		public string CheckerLog { get; set; }

		public string CompilerLog { get; set; }

		public string ValidatorLog { get; set; }
	}
}