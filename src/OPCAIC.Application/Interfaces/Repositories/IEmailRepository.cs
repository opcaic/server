using System.Threading;
using System.Threading.Tasks;
using OPCAIC.Infrastructure.Dtos.Emails;

namespace OPCAIC.Infrastructure.Repositories.Emails
{
	public interface IEmailRepository
	{
		Task<long> EnqueueEmailAsync(NewEmailDto dto, CancellationToken cancellationToken);
		Task<EmailPreviewDto[]> GetEmailsAsync(CancellationToken cancellationToken);
		Task UpdateResultAsync(long id, EmailResultDto dto, CancellationToken cancellationToken);
		Task<EmailPreviewDto[]> GetEmailsToSendAsync(CancellationToken cancellationToken);
	}
}