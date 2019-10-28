using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using OPCAIC.Application.Emails;
using OPCAIC.Application.Emails.Templates;

namespace OPCAIC.FunctionalTest.Infrastructure
{
	public class NullEmailService : IEmailService
	{
		private Dictionary<string, List<EmailData>> emails = new Dictionary<string, List<EmailData>>();

		public NullEmailService()
		{
			
		}

		/// <inheritdoc />
		public Task EnqueueEmailAsync(EmailData data, string recipientEmail, CancellationToken cancellationToken)
		{
			if (!emails.TryGetValue(recipientEmail, out var list))
			{
				emails[recipientEmail] = list = new List<EmailData>();
			}

			list.Add(data);

			return Task.CompletedTask;
		}

		public IEnumerable<EmailData> GetAllEmails(string email)
		{
			return emails[email];
		}
	}
}