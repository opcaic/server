using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MailKit.Net.Smtp;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MimeKit;
using MimeKit.Text;
using OPCAIC.Application.Specifications;
using OPCAIC.Common;
using OPCAIC.Domain.Entities;

namespace OPCAIC.Infrastructure.Emails
{
	public interface IEmailSender : IDisposable
	{
		Task SendAsync(Email email, CancellationToken cancellationToken);
	}

	public class EmailSender
		: IEmailSender
	{
		private readonly EmailsConfiguration configuration;
		private readonly ILogger logger;
		private readonly SmtpClient client;

		public EmailSender(IOptions<EmailsConfiguration> options, ILogger<EmailSender> logger)
		{
			client = new SmtpClient();
			configuration = options.Value;
			this.logger = logger;
		}

		private async Task ConnectAsync(CancellationToken cancellationToken)
		{
			await client.ConnectAsync(configuration.SmtpServerUrl, configuration.Port,
				configuration.UseSsl, cancellationToken);
			await client.AuthenticateAsync(configuration.UserName,
				configuration.Password, cancellationToken);

			logger.LogInformation("Connection to SMTP server established.");
		}

		public async Task SendAsync(Email email, CancellationToken cancellationToken)
		{
			if (!client.IsConnected)
			{
				await ConnectAsync(cancellationToken);
			}

			var mail = new MimeMessage
			{
				Body = new TextPart(TextFormat.Html) { Text = email.Body },
				Subject = email.Subject,
				Sender = new MailboxAddress(configuration.SenderAddress)
			};
			mail.To.Add(InternetAddress.Parse(email.RecipientEmail));

			await client.SendAsync(mail, cancellationToken);
		}

		/// <inheritdoc />
		public void Dispose()
		{
			client.Dispose();
		}
	}
}