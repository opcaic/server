namespace OPCAIC.Application.Emails.Templates
{
	public partial class EmailType
	{
		public static readonly SubmissionValidationFailedType SubmissionValidationFailed =
			CreateDerived<SubmissionValidationFailedType>();

		public class SubmissionValidationFailedType : EmailType
		{
			public SubmissionValidationFailedType()
				:base(typeof(Email))
			{
				
			}

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