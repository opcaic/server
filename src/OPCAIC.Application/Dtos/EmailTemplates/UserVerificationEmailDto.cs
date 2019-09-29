namespace OPCAIC.Application.Dtos.EmailTemplates
{
	public class UserVerificationEmailDto : EmailDtoBase
	{
		/// <inheritdoc />
		public UserVerificationEmailDto(string verificationUrl)
		{
			VerificationUrl = verificationUrl;
		}

		public override string TemplateName => "userVerificationEmail";

		public string VerificationUrl { get; }
	}
}