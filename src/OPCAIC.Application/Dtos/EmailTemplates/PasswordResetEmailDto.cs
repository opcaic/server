namespace OPCAIC.Application.Dtos.EmailTemplates
{
	public class PasswordResetEmailDto : EmailDtoBase
	{
		public override string TemplateName => "passwordResetEmail";

		public string ResetUrl { get; set; }
	}
}