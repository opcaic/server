using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using OPCAIC.Infrastructure.Dtos;
using OPCAIC.Infrastructure.Dtos.Matches;
using OPCAIC.Infrastructure.Entities;

namespace OPCAIC.Infrastructure.Repositories
{
	public interface IMatchRepository : IRepository<Match>
	{
		IEnumerable<Match> AllMatchesFromTournament(long tournamentId);

		Task<IEnumerable<Match>> AllMatchesFromTournamentAsync(long tournamentId,
			CancellationToken cancellationToken = default);

		Task<MatchDetailDto> FindByIdAsync(long id, CancellationToken cancellationToken);

		Task<ListDto<MatchDetailDto>> GetByFilterAsync(MatchFilterDto filter,
			CancellationToken cancellationToken);

		// TODO: standalone repository for match executions?
		Task<MatchExecutionStorageDto> FindExecutionForStorageAsync(long id,
			CancellationToken cancellationToken = default);
	}
}