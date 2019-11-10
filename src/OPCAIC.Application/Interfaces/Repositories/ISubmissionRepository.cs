using System.Threading;
using System.Threading.Tasks;
using OPCAIC.Application.Dtos.Base;
using OPCAIC.Application.Dtos.BaseDtos;
using OPCAIC.Application.Dtos.Submissions;
using OPCAIC.Application.Specifications;
using OPCAIC.Domain.Entities;

namespace OPCAIC.Application.Interfaces.Repositories
{
	public interface ISubmissionRepository
		: IGenericRepository<SubmissionDetailDto, NewSubmissionDto, UpdateSubmissionScoreDto>,
			IRepository<Submission>
	{
		Task<SubmissionDtoBase> FindSubmissionForStorageAsync(long id,
			CancellationToken cancellationToken = default);
	}
}