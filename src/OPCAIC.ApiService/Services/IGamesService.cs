using System.Threading;
using System.Threading.Tasks;
using OPCAIC.ApiService.Models;
using OPCAIC.ApiService.Models.Games;
using OPCAIC.ApiService.Models.Users;

namespace OPCAIC.ApiService.Services
{
	public interface IGamesService
	{
		Task<long> CreateAsync(NewGameModel game, CancellationToken cancellationToken);

		Task<ListModel<GamePreviewModel>> GetByFilterAsync(GameFilterModel filter, CancellationToken cancellationToken);

		Task<GameDetailModel> GetByIdAsync(long id, CancellationToken cancellationToken);

		Task UpdateAsync(long id, UpdateGameModel model, CancellationToken cancellationToken);
	}
}