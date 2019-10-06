using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MailKit.Net.Smtp;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MimeKit;
using OPCAIC.Application.Specifications;
using OPCAIC.Common;
using OPCAIC.Domain.Entities;

namespace OPCAIC.Infrastructure.Emails
{
	public class EmailSender
	{
		private readonly EmailsConfiguration configuration;
		private readonly IRepository<Email> emailRepository;
		private readonly ILogger logger;
		private readonly ITimeService time;

		public EmailSender(IOptions<EmailsConfiguration> options,
			IRepository<Email> emailRepository,
			ILogger<EmailSender> logger, ITimeService time)
		{
			configuration = options.Value;
			this.emailRepository = emailRepository;
			this.logger = logger;
			this.time = time;
		}

		public async Task TickAsync(CancellationToken cancellationToken)
		{
			var totalSent = 0;

			var spec = new BaseSpecification<Email>()
				.AddCriteria(e => e.RemainingAttempts > 0 &&
					e.SentAt == null)
				.Ordered(e => e.Created);

			List<Email> emails;
			while ((emails = await emailRepository.ListAsync(spec, cancellationToken)).Count > 0)
			{
				using (var client = new SmtpClient())
				{
					try
					{
						await client.ConnectAsync(configuration.SmtpServerUrl, configuration.Port,
							configuration.UseSsl, cancellationToken);
						await client.AuthenticateAsync(configuration.UserName,
							configuration.Password, cancellationToken);

						logger.LogInformation("Connection to SMTP server established.");
					}
					catch (Exception ex)
					{
						logger.LogError(ex, "Establishing connection to SMTP server failed.");
						return;
					}

					foreach (var email in emails)
					{
						try
						{
							var mail = new MimeMessage
							{
								Body = new TextPart {Text = email.Body},
								Subject = email.Subject,
								Sender = new MailboxAddress(configuration.SenderAddress)
							};
							mail.To.Add(InternetAddress.Parse(email.RecipientEmail));

							await client.SendAsync(mail, cancellationToken);

							logger.LogInformation(LoggingEvents.MailSentSuccess,
								$"Email with id '{{{LoggingTags.MailId}}}' successfully sent to '{{{LoggingTags.UserEmail}}}'",
								email.Id, email.RecipientEmail);

							email.SentAt = time.Now;
							await emailRepository.SaveChangesAsync(cancellationToken);

							totalSent++;
						}
						catch (Exception ex)
						{
							email.RemainingAttempts--;
							await emailRepository.SaveChangesAsync(cancellationToken);

							logger.LogWarning(LoggingEvents.MailSentFailed, ex,
								$"Sending email with id '{{{LoggingTags.MailId}}}' to user '{{{LoggingTags.UserEmail}}}' failed.",
								email.Id, email.RecipientEmail);
						}
					}
				}
			}

			if (totalSent > 0)
			{
				logger.LogInformation("{totalSent} emails was succesfully sent.", totalSent);
			}
		}
	}
}