using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OPCAIC.Application.Specifications;
using OPCAIC.Common;
using OPCAIC.Domain.Entities;
using OPCAIC.Infrastructure.Emails;

namespace OPCAIC.ApiService.Services
{
	public class EmailCronService : HostedJob
	{
		private const int TickMilliseconds = 5000;

		private static readonly ISpecification<Email> emailsToSendSpecification = 
			new BaseSpecification<Email>()
				.AddCriteria(e => e.RemainingAttempts > 0 && e.SentAt == null)
				.Ordered(e => e.Created);

		public EmailCronService(IServiceScopeFactory scopeFactory, ILogger<EmailCronService> logger) 
			: base(scopeFactory, logger, TimeSpan.FromMilliseconds(TickMilliseconds))
		{
		}

		protected override async Task ExecuteJob(IServiceProvider scopedProvider,
			CancellationToken cancellationToken)
		{
			var totalSent = 0;
			var repository = scopedProvider.GetRequiredService<IRepository<Email>>();
			var time = scopedProvider.GetRequiredService<ITimeService>();

			List<Email> emails;
			while ((emails =
					await repository.ListAsync(emailsToSendSpecification, cancellationToken))
				.Count > 0)
			{
				using var sender = scopedProvider.GetRequiredService<IEmailSender>();

				foreach (var email in emails)
				{
					try
					{
						await sender.SendAsync(email, cancellationToken);

						Logger.LogInformation(LoggingEvents.MailSentSuccess,
							$"Email with id '{{{LoggingTags.MailId}}}' successfully sent to '{{{LoggingTags.UserEmail}}}'",
							email.Id, email.RecipientEmail);

						email.SentAt = time.Now;
						await repository.SaveChangesAsync(cancellationToken);
						totalSent++;
					}
					catch (Exception ex)
					{
						email.RemainingAttempts--;
						await repository.SaveChangesAsync(cancellationToken);

						Logger.LogWarning(LoggingEvents.MailSentFailed, ex,
							$"Sending email with id '{{{LoggingTags.MailId}}}' to user '{{{LoggingTags.UserEmail}}}' failed.",
							email.Id, email.RecipientEmail);
					}
				}
			}

			if (totalSent > 0)
			{
				Logger.LogInformation("{totalSent} emails was successfully sent.", totalSent);
			}
		}
	}
}