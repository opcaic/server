using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using OPCAIC.Domain.Entities;
using OPCAIC.Infrastructure.Emails;

namespace OPCAIC.ApiService.Services.Development
{
	public class LoggingEmailSender : IEmailSender
	{
		private ILogger<LoggingEmailSender> logger;

		public LoggingEmailSender(ILogger<LoggingEmailSender> logger)
		{
			this.logger = logger;
		}

		/// <inheritdoc />
		public void Dispose()
		{
		}

		/// <inheritdoc />
		public Task SendAsync(Email email, CancellationToken cancellationToken)
		{
			logger.LogInformation($"Would send email to {email.RecipientEmail}:\nSubject: {email.Subject}\nBody:\n{email.Body}");
			return Task.CompletedTask;
		}
	}
}