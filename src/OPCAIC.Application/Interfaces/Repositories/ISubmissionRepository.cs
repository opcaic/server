using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using OPCAIC.Application.Dtos.Submissions;
using OPCAIC.Application.Specifications;
using OPCAIC.Domain.Entities;

namespace OPCAIC.Application.Interfaces.Repositories
{
	public interface ISubmissionRepository
		: IGenericRepository<SubmissionDetailDto, NewSubmissionDto, UpdateSubmissionScoreDto>,
			IAuthDataRepository<SubmissionAuthDto>,
			IRepository<Submission>
	{
		Task<SubmissionStorageDto> FindSubmissionForStorageAsync(long id,
			CancellationToken cancellationToken = default);

		Task<List<SubmissionDetailDto>> AllSubmissionsFromTournament(long tournamentId,
			CancellationToken cancellationToken);
	}
}