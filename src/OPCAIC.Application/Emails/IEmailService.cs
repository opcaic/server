using System.Threading;
using System.Threading.Tasks;
using OPCAIC.Application.Emails.Templates;

namespace OPCAIC.Application.Emails
{
	public interface IEmailService
	{
		Task EnqueueEmailAsync(EmailData data, string recipientEmail, CancellationToken cancellationToken);
	}
}