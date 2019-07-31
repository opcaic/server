using System.Threading;
using System.Threading.Tasks;

namespace OPCAIC.Infrastructure.Repositories
{
	public interface IUserTournamentRepository
	{
		Task<long[]> FindTournamentsByUserAsync(long userId, CancellationToken cancellationToken);
		Task CreateAsync(long userId, long tournamentId, CancellationToken cancellationToken);
	}
}