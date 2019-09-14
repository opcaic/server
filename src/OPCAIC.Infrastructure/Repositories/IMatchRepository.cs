using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using OPCAIC.Infrastructure.Dtos;
using OPCAIC.Infrastructure.Dtos.Matches;

namespace OPCAIC.Infrastructure.Repositories
{
	public interface IMatchRepository
		: ILookupRepository<MatchDetailDto>,
			IFilterRepository<MatchFilterDto, MatchDetailDto>,
			IAuthDataRepository<MatchAuthDto>
	{
		Task CreateMatchesAsync(List<NewMatchDto> matches, CancellationToken cancellationToken);
		Task<List<MatchDetailDto>> AllMatchesFromTournamentAsync(long tournamentId, CancellationToken cancellationToken);
	}
}