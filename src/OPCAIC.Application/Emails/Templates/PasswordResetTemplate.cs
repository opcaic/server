namespace OPCAIC.Application.Emails.Templates
{
	public partial class EmailType
	{
		public static readonly PasswordResetTemplate
			PasswordReset = Create<PasswordResetTemplate>();

		public class PasswordResetTemplate : Type<PasswordResetTemplate.Email>
		{
			public Email CreateEmail(string resetUrl)
			{
				return new Email(Name, resetUrl);
			}

			public class Email : EmailData
			{
				/// <inheritdoc />
				public Email(string templateName, string resetUrl)
				{
					TemplateName = templateName;
					ResetUrl = resetUrl;
				}

				public override string TemplateName { get; }

				public string ResetUrl { get; }
			}
		}
	}
}