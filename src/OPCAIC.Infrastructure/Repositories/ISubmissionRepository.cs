using System.Threading;
using System.Threading.Tasks;
using OPCAIC.Infrastructure.Dtos;
using OPCAIC.Infrastructure.Dtos.Submissions;

namespace OPCAIC.Infrastructure.Repositories
{
	public interface ISubmissionRepository
		: IGenericRepository<SubmissionFilterDto, SubmissionPreviewDto, SubmissionDetailDto,
			NewSubmissionDto, UpdateSubmissionDto>
	{
		Task<SubmissionStorageDto> FindSubmissionForStorageAsync(long id,
			CancellationToken cancellationToken = default);
	}
}