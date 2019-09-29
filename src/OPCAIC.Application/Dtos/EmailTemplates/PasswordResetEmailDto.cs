namespace OPCAIC.Application.Dtos.EmailTemplates
{
	public class PasswordResetEmailDto : EmailDtoBase
	{
		public PasswordResetEmailDto(string resetUrl)
		{
			ResetUrl = resetUrl;
		}

		public override string TemplateName => "passwordResetEmail";

		public string ResetUrl { get; }
	}
}