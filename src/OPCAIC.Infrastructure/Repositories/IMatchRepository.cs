using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using OPCAIC.Infrastructure.Dtos;
using OPCAIC.Infrastructure.Dtos.Matches;
using OPCAIC.Infrastructure.Entities;

namespace OPCAIC.Infrastructure.Repositories
{
	public interface IMatchRepository
		: ILookupRepository<MatchDetailDto>,
			IFilterRepository<MatchFilterDto, MatchDetailDto>,
			IAuthDataRepository<MatchAuthDto>
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
	}
}