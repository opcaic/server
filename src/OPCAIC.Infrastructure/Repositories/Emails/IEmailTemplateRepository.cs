using OPCAIC.Infrastructure.Dtos.EmailTemplates;
using System.Threading;
using System.Threading.Tasks;

namespace OPCAIC.Infrastructure.Repositories.Emails
{
	public interface IEmailTemplateRepository
	{
		Task<EmailTemplateDto> GetTemplateAsync(string templateName, string lngCode, CancellationToken cancellationToken);
	}
}
