using System.Threading;
using System.Threading.Tasks;
using OPCAIC.Infrastructure.Dtos;

namespace OPCAIC.Infrastructure.Repositories
{
	public interface ISubmissionRepository
	{
		Task<SubmissionStorageDto> FindSubmissionForStorageAsync(long id,
			CancellationToken cancellationToken = default);
	}
}