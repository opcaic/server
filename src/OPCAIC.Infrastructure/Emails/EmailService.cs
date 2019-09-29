﻿using System.Threading;
using System.Threading.Tasks;
using HandlebarsDotNet;
using OPCAIC.Application.Dtos.Emails;
using OPCAIC.Application.Dtos.EmailTemplates;
using OPCAIC.Application.Dtos.Users;
using OPCAIC.Application.Emails;
using OPCAIC.Application.Extensions;
using OPCAIC.Application.Interfaces.Repositories;

namespace OPCAIC.Infrastructure.Emails
{
	public class EmailService : IEmailService
	{
		private readonly IEmailRepository emailRepository;
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

		public async Task EnqueueEmailAsync(EmailDtoBase dto, string recipientEmail,
			CancellationToken cancellationToken)
		{
			var user = await userRepository.GetAsync(r => r.Email == recipientEmail,
				p => new EmailRecipientDto
				{
					Email = p.Email,
					LocalizationLanguage = p.LocalizationLanguage
				}, cancellationToken);

#warning Default language code needs to be placed somewhere to configuration or to constant
			await EnqueueEmailAsync(dto, recipientEmail, user?.LocalizationLanguage ?? "en",
				cancellationToken);
		}

		private async Task EnqueueEmailAsync(EmailDtoBase dto, string recipientEmail,
			string lngCode, CancellationToken cancellationToken)
		{
			var template =
				await emailTemplateRepository.GetTemplateAsync(dto.TemplateName, lngCode,
					cancellationToken);

			var body = handlebars.Compile(template.BodyTemplate)(dto);
			var subject = handlebars.Compile(template.SubjectTemplate)(dto);

			var email = new NewEmailDto
			{
				Body = body,
				Subject = subject,
				RecipientEmail = recipientEmail,
				TemplateName = dto.TemplateName
			};

			await emailRepository.EnqueueEmailAsync(email, cancellationToken);
		}
	}
}