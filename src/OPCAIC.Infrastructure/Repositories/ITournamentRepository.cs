using OPCAIC.Infrastructure.Dtos.Tournaments;

namespace OPCAIC.Infrastructure.Repositories
{
	public interface ITournamentRepository
		: IGenericRepository<TournamentFilterDto, TournamentPreviewDto, TournamentDetailDto,
			NewTournamentDto, UpdateTournamentDto>
	{
		Task<long> CreateAsync(NewTournamentDto tournament, CancellationToken cancellationToken);

		Task<ListDto<TournamentPreviewDto>> GetByFilterAsync(TournamentFilterDto filter,
			CancellationToken cancellationToken);

		Task<TournamentDetailDto> FindByIdAsync(long id, CancellationToken cancellationToken);

		Task<bool> UpdateAsync(long id, UpdateTournamentDto dto,
			CancellationToken cancellationToken);
		
		Task<bool> CheckTournamentExists(long id);
	}
}