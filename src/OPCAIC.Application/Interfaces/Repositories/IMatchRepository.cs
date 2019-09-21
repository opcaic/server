using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using OPCAIC.Application.Dtos.Matches;
using OPCAIC.Application.Matches.Models;
using OPCAIC.Application.Specifications;
using OPCAIC.Domain.Entities;

namespace OPCAIC.Application.Interfaces.Repositories
{
	public interface IMatchRepository
		: ILookupRepository<MatchDetailDto>,
			IAuthDataRepository<MatchAuthDto>,
			IRepository<Match>
	{
		Task CreateMatchesAsync(List<NewMatchDto> matches, CancellationToken cancellationToken);
		Task<List<MatchDetailDto>> AllMatchesFromTournamentAsync(long tournamentId, CancellationToken cancellationToken);
	}
}