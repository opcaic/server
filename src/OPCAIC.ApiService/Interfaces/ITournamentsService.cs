using System.Threading;
using System.Threading.Tasks;
using OPCAIC.ApiService.Models.Tournaments;
using OPCAIC.Application.Tournaments.Models;

namespace OPCAIC.ApiService.Interfaces
{
	public interface ITournamentsService
	{
		Task<long> CreateAsync(NewTournamentModel tournament, CancellationToken cancellationToken);

		Task<TournamentDetailModel> GetByIdAsync(long id, CancellationToken cancellationToken);

		Task UpdateAsync(long id, UpdateTournamentModel model, CancellationToken cancellationToken);
	}
}