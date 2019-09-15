using System.Threading;
using System.Threading.Tasks;
using OPCAIC.ApiService.Models;
using OPCAIC.ApiService.Models.Tournaments;

namespace OPCAIC.ApiService.Interfaces
{
	public interface ITournamentsService
	{
		Task<bool> ExistsByIdAsync(long id, CancellationToken cancellationToken);

		Task<long> CreateAsync(NewTournamentModel tournament, CancellationToken cancellationToken);

		Task<ListModel<TournamentPreviewModel>> GetByFilterAsync(TournamentFilterModel filter,
			CancellationToken cancellationToken);

		Task<TournamentDetailModel> GetByIdAsync(long id, CancellationToken cancellationToken);

		Task UpdateAsync(long id, UpdateTournamentModel model, CancellationToken cancellationToken);
		Task StartTournamentEvaluation(long id, CancellationToken cancellationToken);
	}
}