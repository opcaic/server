using System.Threading;
using System.Threading.Tasks;
using OPCAIC.Infrastructure.Dtos.EmailTemplates;

namespace OPCAIC.Infrastructure.Repositories.Emails
{
	public interface IEmailTemplateRepository
	{
		Task<EmailTemplateDto> GetTemplateAsync(string templateName, string lngCode,
			CancellationToken cancellationToken);
	}
}