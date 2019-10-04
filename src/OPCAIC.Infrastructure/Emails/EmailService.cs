using System.Threading;
using System.Threading.Tasks;
using HandlebarsDotNet;
using OPCAIC.Application.Dtos.Users;
using OPCAIC.Application.Emails;
using OPCAIC.Application.Emails.Templates;
using OPCAIC.Application.Extensions;
using OPCAIC.Application.Interfaces.Repositories;
using OPCAIC.Application.Specifications;
using OPCAIC.Domain.Entities;
using OPCAIC.Domain.Enumerations;

namespace OPCAIC.Infrastructure.Emails
{
	public class EmailService : IEmailService
	{
		private readonly IRepository<Email> emailRepository;
		private readonly IEmailTemplateRepository emailTemplateRepository;

		private readonly IHandlebars handlebars = Handlebars.Create();
		private readonly IUserRepository userRepository;

		public EmailService(IEmailRepository emailRepository,
			IEmailTemplateRepository emailTemplateRepository, IUserRepository userRepository)
		{
			this.emailRepository = emailRepository;
			this.emailTemplateRepository = emailTemplateRepository;
			this.userRepository = userRepository;
		}

		public async Task EnqueueEmailAsync(EmailData data, string recipientEmail,
			CancellationToken cancellationToken)
		{
			var recipient = await userRepository.GetAsync(r => r.Email == recipientEmail,
				p => new { p.Email, p.LocalizationLanguage }, cancellationToken);

			await EnqueueEmailAsync(data, recipientEmail,
				recipient?.LocalizationLanguage ?? LocalizationLanguage.EN,
				cancellationToken);
		}

		private async Task EnqueueEmailAsync(EmailData data, string recipientEmail,
			string lngCode, CancellationToken cancellationToken)
		{
			var template =
				await emailTemplateRepository.GetTemplateAsync(data.TemplateName, lngCode,
					cancellationToken);

			var body = handlebars.Compile(template.BodyTemplate)(data);
			var subject = handlebars.Compile(template.SubjectTemplate)(data);

			var email = new Email
			{
				Body = body,
				Subject = subject,
				RecipientEmail = recipientEmail,
				RemainingAttempts = 3,
				TemplateName = data.TemplateName
			};

			await emailRepository.CreateAsync(email, cancellationToken);
		}
	}
}