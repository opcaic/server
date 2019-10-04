namespace OPCAIC.Application.Emails.Templates
{
	public partial class EmailType
	{
		public static readonly SubmissionValidationFailedType SubmissionValidationFailed =
			Create<SubmissionValidationFailedType>();

		public class SubmissionValidationFailedType
			: Type<SubmissionValidationFailedType.Email>
		{
			public Email CreateEmail(string submissionUrl)
			{
				return new Email(Name, submissionUrl);
			}

			public class Email : EmailData
			{
				/// <inheritdoc />
				public Email(string templateName, string submissionUrl)
				{
					TemplateName = templateName;
					SubmissionUrl = submissionUrl;
				}

				/// <inheritdoc />
				public override string TemplateName { get; }

				public string SubmissionUrl { get; }
			}
		}
	}
}