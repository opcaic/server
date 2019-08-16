namespace OPCAIC.Infrastructure.Dtos.EmailTemplates
{
	public class UserVerificationEmailDto : EmailDtoBase
	{
		public override string TemplateName => "userVerificationEmail";

		public string VerificationUrl { get; set; }
	}
}