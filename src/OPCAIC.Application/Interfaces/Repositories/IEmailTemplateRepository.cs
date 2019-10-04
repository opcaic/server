using System.Threading;
using System.Threading.Tasks;
using OPCAIC.Application.Emails.Templates;

namespace OPCAIC.Application.Interfaces.Repositories
{
	public interface IEmailTemplateRepository
	{
		Task<EmailTemplateDto> GetTemplateAsync(string templateName, string lngCode,
			CancellationToken cancellationToken);
	}
}