using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using OPCAIC.Infrastructure.Dtos;
using OPCAIC.Infrastructure.Dtos.Matches;
using OPCAIC.Infrastructure.Entities;

namespace OPCAIC.Infrastructure.Repositories
{
	public interface IMatchRepository
		: ILookupRepository<MatchFilterDto, MatchDetailDto, MatchDetailDto>
	{
		/// <summary>
		///     Returns all matches from the tournament identified by given id.
		/// </summary>
		/// <param name="tournamentId">Id of the tournament.</param>
		/// <returns></returns>
		IEnumerable<Match> AllMatchesFromTournament(long tournamentId);

		/// <summary>
		///     Returns all matches from the tournament identified by given id as an asynchronous operation.
		/// </summary>
		/// <param name="tournamentId">Id of the tournament.</param>
		/// <returns></returns>
		Task<IEnumerable<Match>> AllMatchesFromTournamentAsync(long tournamentId,
			CancellationToken cancellationToken = default);

		Task<MatchDetailDto> FindByIdAsync(long id, CancellationToken cancellationToken);

		Task<ListDto<MatchDetailDto>> GetByFilterAsync(MatchFilterDto filter,
			CancellationToken cancellationToken);

		// TODO: standalone repository for match executions?
		/// <summary>
		///     Returns data needed to find where the archive with results of match execution with given id is stored.
		/// </summary>
		/// <param name="id">Id of the match execution.</param>
		/// <param name="cancellationToken"></param>
		/// <returns></returns>
		Task<MatchExecutionStorageDto> FindExecutionForStorageAsync(long id,
			CancellationToken cancellationToken = default);
	}
}