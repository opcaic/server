using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using OPCAIC.Application.Dtos.Matches;

namespace OPCAIC.Application.Interfaces.Repositories
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