using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using OPCAIC.Application.Dtos.Submissions;

namespace OPCAIC.Application.Interfaces.Repositories
{
	public interface ISubmissionRepository
		: IGenericRepository<SubmissionFilterDto, SubmissionPreviewDto, SubmissionDetailDto,
				NewSubmissionDto, UpdateSubmissionScoreDto>,
			IAuthDataRepository<SubmissionAuthDto>
	{
		Task<SubmissionStorageDto> FindSubmissionForStorageAsync(long id,
			CancellationToken cancellationToken = default);

		Task<List<SubmissionDetailDto>> AllSubmissionsFromTournament(long tournamentId,
			CancellationToken cancellationToken);
	}
}