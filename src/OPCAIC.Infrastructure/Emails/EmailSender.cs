using System;
using System.Threading;
using System.Threading.Tasks;
using MailKit.Net.Smtp;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MimeKit;
using OPCAIC.Infrastructure.Dtos.Emails;
using OPCAIC.Infrastructure.Repositories.Emails;
using OPCAIC.Utils;

namespace OPCAIC.Infrastructure.Emails
{
	public class EmailSender
	{
		private readonly EmailsConfiguration configuration;
		private readonly IEmailRepository emailRepository;
		private readonly ILogger logger;

		public EmailSender(IOptions<EmailsConfiguration> options, IEmailRepository emailRepository, ILogger<EmailSender> logger)
		{
			this.configuration = options.Value;
			this.emailRepository = emailRepository;
			this.logger = logger;
		}

		public async Task TickAsync(CancellationToken cancellationToken)
		{
			int totalSent = 0;
			EmailPreviewDto[] emails;
			do
			{
				emails = await emailRepository.GetEmailsToSendAsync(cancellationToken);
				if (emails.Length == 0)
					break;

				using (var client = new SmtpClient())
				{
					try
					{
						await client.ConnectAsync(configuration.SmtpServerUrl, configuration.Port, configuration.UseSsl, cancellationToken);
						await client.AuthenticateAsync(configuration.UserName, configuration.Password, cancellationToken);

						logger.LogInformation($"Connection to SMTP server established.");
					}
					catch (Exception ex)
					{
						logger.LogError(ex, $"Establishing connection to SMTP server failed.");
						return;
					}

					foreach (var email in emails)
					{
						try
						{
							var mail = new MimeMessage
							{
								Body = new TextPart { Text = email.Body },
								Subject = email.Subject,
								Sender = new MailboxAddress(configuration.SenderAddress)
							};
							mail.To.Add(InternetAddress.Parse(email.RecipientEmail));

							await client.SendAsync(mail, cancellationToken);

							logger.LogInformation(LoggingEvents.MailSentSuccess, $"Email with id '{{{LoggingTags.MailId}}}' succesfully sent to '{{{LoggingTags.UserEmail}}}'", email.Id, email.RecipientEmail);
							await emailRepository.UpdateResultAsync(email.Id, new EmailResultDto { SentAt = DateTime.Now }, cancellationToken);
						}
						catch (Exception ex)
						{
							var result = new EmailResultDto { RemainingAttempts = email.RemainingAttempts - 1 };
							await emailRepository.UpdateResultAsync(email.Id, result, cancellationToken);

							logger.LogWarning(LoggingEvents.MailSentFailed, ex, $"Sending email with id '{{{LoggingTags.MailId}}}' to user '{{{LoggingTags.UserEmail}}}' failed.", email.Id, email.RecipientEmail);
						}
					}
				}

				totalSent += emails.Length;
			}
			while (emails.Length > 0);

			logger.LogInformation("{totalSent} emails was succesfully sent.", totalSent);
		}
	}
}
