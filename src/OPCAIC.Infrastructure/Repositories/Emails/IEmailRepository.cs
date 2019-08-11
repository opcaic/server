using OPCAIC.Infrastructure.Dtos.Emails;
using System.Threading;
using System.Threading.Tasks;

namespace OPCAIC.Infrastructure.Repositories.Emails
{
	public interface IEmailRepository
	{
		Task<long> EnqueueEmailAsync(NewEmailDto dto, CancellationToken cancellationToken);
		Task<EmailPreviewDto[]> GetEmailsAsync(CancellationToken cancellationToken);
	}
}
