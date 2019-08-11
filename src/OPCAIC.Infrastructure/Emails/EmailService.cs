using HandlebarsDotNet;
using OPCAIC.Infrastructure.Dtos.Emails;
using OPCAIC.Infrastructure.Dtos.EmailTemplates;
using OPCAIC.Infrastructure.Repositories;
using OPCAIC.Infrastructure.Repositories.Emails;
using System.Threading;
using System.Threading.Tasks;

namespace OPCAIC.Infrastructure.Emails
{
	public class EmailService: IEmailService
	{
		private readonly IEmailRepository emailRepository;
		private readonly IEmailTemplateRepository emailTemplateRepository;
		private readonly IUserRepository userRepository;

		private readonly IHandlebars handlebars = Handlebars.Create();

		public EmailService(IEmailRepository emailRepository, IEmailTemplateRepository emailTemplateRepository, IUserRepository userRepository)
		{
			this.emailRepository = emailRepository;
			this.emailTemplateRepository = emailTemplateRepository;
			this.userRepository = userRepository;
		}

		private async Task EnqueueEmailAsync(EmailDtoBase dto, string recipientEmail, string lngCode, CancellationToken cancellationToken)
		{
			var template = await emailTemplateRepository.GetTemplateAsync(dto.TemplateName, lngCode, cancellationToken);

			string body = handlebars.Compile(template.BodyTemplate)(dto);
			string subject = handlebars.Compile(template.SubjectTemplate)(dto);

			var emaildto = new NewEmailDto
			{
				Body = body,
				Subject = subject,
				RecipientEmail = recipientEmail,
				TemplateName = dto.TemplateName
			};

			await emailRepository.EnqueueEmailAsync(emaildto, cancellationToken);
		}

		public async Task SendEmailVerificationEmailAsync(long recipientId, string verificationUrl, CancellationToken cancellationToken)
		{
			var user = await userRepository.FindRecipientAsync(recipientId, cancellationToken);

			var emailDto = new UserVerificationEmailDto { VerificationUrl = verificationUrl };

			await EnqueueEmailAsync(emailDto, user.Email, user.LocalizationLanguage, cancellationToken);
		} 

		public async Task SendPasswordResetEmailAsync(string recipientEmail, string resetUrl, CancellationToken cancellationToken)
		{
			var user = await userRepository.FindRecipientAsync(recipientEmail, cancellationToken);

			var emailDto = new PasswordResetEmailDto { ResetUrl = resetUrl };

			await EnqueueEmailAsync(emailDto, user.Email, user.LocalizationLanguage, cancellationToken);
		}
	}
}
