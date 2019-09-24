using System.IO;
using System.Threading;
using System.Threading.Tasks;
using OPCAIC.ApiService.Models.Submissions;

namespace OPCAIC.ApiService.Interfaces
{
	public interface ISubmissionService
	{
		Task<Stream> GetSubmissionArchiveAsync(long id, CancellationToken cancellationToken);

		Task<bool> ExistsByIdAsync(long id, CancellationToken cancellationToken);
	}
}