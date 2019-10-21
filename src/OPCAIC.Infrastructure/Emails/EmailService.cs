using System;
using System.Threading;
using System.Threading.Tasks;
using HandlebarsDotNet;
using Microsoft.Extensions.Logging;
using OPCAIC.Application.Dtos.Users;
using OPCAIC.Application.Emails;
using OPCAIC.Application.Emails.Templates;
using OPCAIC.Application.Extensions;
using OPCAIC.Application.Interfaces.Repositories;
using OPCAIC.Application.Logging;
using OPCAIC.Application.Specifications;
using OPCAIC.Domain.Entities;
using OPCAIC.Domain.Enumerations;

namespace OPCAIC.Infrastructure.Emails
{
	public class EmailService : IEmailService
	{
		private readonly ILogger<EmailService> logger;
		private readonly IRepository<Email> emailRepository;
		private readonly IRepository<EmailTemplate> emailTemplateRepository;

		private readonly IHandlebars handlebars = Handlebars.Create();
		private readonly IUserRepository userRepository;

		public EmailService(IRepository<Email> emailRepository,
			IRepository<EmailTemplate> emailTemplateRepository, IUserRepository userRepository, ILogger<EmailService> logger)
		{
			this.emailRepository = emailRepository;
			this.emailTemplateRepository = emailTemplateRepository;
			this.userRepository = userRepository;
			this.logger = logger;
		}

		public async Task EnqueueEmailAsync(EmailData data, string recipientEmail,
			CancellationToken cancellationToken)
		{
			var language = await userRepository.FindAsync(r => r.Email == recipientEmail,
				p => p.LocalizationLanguage, cancellationToken);

			await EnqueueEmailAsync(data, recipientEmail,
				language ?? LocalizationLanguage.EN,
				cancellationToken);
		}

		private async Task EnqueueEmailAsync(EmailData data, string recipientEmail,
			string lngCode, CancellationToken cancellationToken)
		{
			var template = await GetEmailTemplate(data.TemplateName, lngCode, cancellationToken);

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

		private async Task<EmailTemplate> GetEmailTemplate(string type, string lngCode,
			CancellationToken cancellationToken)
		{
			var template =
				await emailTemplateRepository.FindAsync(
					t => t.Name == type && t.LanguageCode == lngCode,
					cancellationToken);

			if (template == null && lngCode != LocalizationLanguage.EN)
			{
				template = await emailTemplateRepository.FindAsync(
					t => t.Name == type && t.LanguageCode == LocalizationLanguage.EN.Name,
					cancellationToken);

				if (template == null)
				{
					throw new InvalidOperationException($"Missing template for language '{lngCode}' and type '{type}'");
				}

				logger.EmailTemplateMissing(type, lngCode);
			}

			return template;
		}
	}
}