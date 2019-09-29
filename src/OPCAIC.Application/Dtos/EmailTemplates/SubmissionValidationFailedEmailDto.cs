namespace OPCAIC.Application.Dtos.EmailTemplates
{
	public class SubmissionValidationFailedEmailDto : EmailDtoBase
	{
		/// <inheritdoc />
		public SubmissionValidationFailedEmailDto(string submissionUrl)
		{
			SubmissionUrl = submissionUrl;
		}

		/// <inheritdoc />
		public override string TemplateName => "submissionValidationFailedEmail";

		public string SubmissionUrl { get; }
	}
}