using System.Threading;
using System.Threading.Tasks;
using OPCAIC.Infrastructure.Dtos;
using OPCAIC.Infrastructure.Dtos.Games;

namespace OPCAIC.Infrastructure.Repositories
{
	public interface IGameRepository
	{
		Task<long> CreateAsync(NewGameDto game, CancellationToken cancellationToken);

		Task<ListDto<GamePreviewDto>> GetByFilterAsync(GameFilterDto filter, CancellationToken cancellationToken);

		Task<GameDetailDto> FindByIdAsync(long id, CancellationToken cancellationToken);

		Task<bool> UpdateAsync(long id, UpdateGameDto dto, CancellationToken cancellationToken);

		Task<bool> ExistsByName(string name, CancellationToken cancellationToken);
	}
}