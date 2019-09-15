using System.Threading;
using System.Threading.Tasks;
using OPCAIC.ApiService.Models.Leaderboards;

namespace OPCAIC.ApiService.Interfaces
{
	public interface ILeaderboardService
	{
		Task<LeaderboardModel> GetTournamentLeaderboard(long tournamentId,
			CancellationToken cancellationToken);
	}
}