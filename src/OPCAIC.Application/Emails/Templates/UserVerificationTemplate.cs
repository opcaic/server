namespace OPCAIC.Application.Emails.Templates
{
	public partial class EmailType
	{
		public static readonly UserVerificationType UserVerification =
			CreateDerived<UserVerificationType>();

		public class UserVerificationType : EmailType
		{
			public UserVerificationType()
				: base(typeof(Email))
			{

			}

			public Email CreateEmail(string verificationUrl)
			{
				return new Email(Name, verificationUrl);
			}

			public class Email : EmailData
			{
				/// <inheritdoc />
				public Email(string templateName, string verificationUrl)
				{
					TemplateName = templateName;
					VerificationUrl = verificationUrl;
				}

				/// <inheritdoc />
				public override string TemplateName { get; }

				public string VerificationUrl { get; }
			}
		}
	}
}