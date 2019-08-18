using System.Threading;
using System.Threading.Tasks;
using OPCAIC.Infrastructure.Dtos;
using OPCAIC.Infrastructure.Dtos.Tournaments;

namespace OPCAIC.Infrastructure.Repositories
{
	public interface ITournamentRepository
	{
		Task<bool> ExistsByIdAsync(long id, CancellationToken cancellationToken);

		Task<long> CreateAsync(NewTournamentDto tournament, CancellationToken cancellationToken);

		Task<ListDto<TournamentPreviewDto>> GetByFilterAsync(TournamentFilterDto filter,
			CancellationToken cancellationToken);

		Task<TournamentDetailDto> FindByIdAsync(long id, CancellationToken cancellationToken);

		Task<bool> UpdateAsync(long id, UpdateTournamentDto dto,
			CancellationToken cancellationToken);
	}
}