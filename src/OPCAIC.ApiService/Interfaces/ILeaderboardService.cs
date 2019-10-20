using System.Threading;
using System.Threading.Tasks;
using OPCAIC.Application.Tournaments.Models;

namespace OPCAIC.ApiService.Interfaces
{
	public interface ILeaderboardService
	{
		Task<TournamentLeaderboardDto> GetTournamentLeaderboard(long tournamentId,
			CancellationToken cancellationToken);
	}
}