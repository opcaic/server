using System.Threading;
using System.Threading.Tasks;
using OPCAIC.Infrastructure.Dtos.EmailTemplates;

namespace OPCAIC.Infrastructure.Emails
{
	public interface IEmailService
	{
		Task EnqueueEmailAsync(EmailDtoBase dto, string recipientEmail, CancellationToken cancellationToken);
	}
}