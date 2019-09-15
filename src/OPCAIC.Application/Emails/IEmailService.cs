using System.Threading;
using System.Threading.Tasks;
using OPCAIC.Application.Dtos.EmailTemplates;

namespace OPCAIC.Application.Emails
{
	public interface IEmailService
	{
		Task EnqueueEmailAsync(EmailDtoBase dto, string recipientEmail, CancellationToken cancellationToken);
	}
}