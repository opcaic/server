using System.Threading;
using System.Threading.Tasks;
using OPCAIC.Infrastructure.Dtos;
using OPCAIC.Infrastructure.Entities;

namespace OPCAIC.Infrastructure.Repositories
{
	public interface ISubmissionRepository : ISimpleLookupRepository<Submission>
	{
		Task<SubmissionStorageDto> FindSubmissionForStorageAsync(long id, CancellationToken cancellationToken = default);
	}
}