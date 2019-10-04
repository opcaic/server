using System.Threading;
using System.Threading.Tasks;
using OPCAIC.Application.Emails.Models;
using OPCAIC.Application.Specifications;
using OPCAIC.Domain.Entities;

namespace OPCAIC.Application.Interfaces.Repositories
{
	public interface IEmailRepository : IRepository<Email>
	{
		Task<EmailDto[]> GetEmailsAsync(CancellationToken cancellationToken);
	}
}