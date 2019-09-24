using System.IO;
using System.Threading;
using System.Threading.Tasks;
using OPCAIC.ApiService.Models.Submissions;

namespace OPCAIC.ApiService.Interfaces
{
	public interface ISubmissionService
	{
		Task<long> CreateAsync(NewSubmissionModel model, long userId,
			CancellationToken cancellationToken);

		Task<Stream> GetSubmissionArchiveAsync(long id, CancellationToken cancellationToken);

		Task<bool> ExistsByIdAsync(long id, CancellationToken cancellationToken);
	}
}