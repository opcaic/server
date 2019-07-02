using System.Collections.Generic;
using System.Threading.Tasks;
using OPCAIC.Infrastructure.Entities;

namespace OPCAIC.Infrastructure.Repository
{
	public interface IMatchRepository : IRepository<Match>
	{
		Match Find(long matchId, long tournamentId);
		Task<Match> FindAsync(long matchId, long tournamentId);
		IEnumerable<Match> AllMatchesFromTournament(long tournamentId);
		Task<IEnumerable<Match>> AllMatchesFromTournamentAsync(long tournamentId);
	}
}