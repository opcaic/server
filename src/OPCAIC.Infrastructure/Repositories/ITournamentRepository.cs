using OPCAIC.Infrastructure.Dtos.Tournaments;

namespace OPCAIC.Infrastructure.Repositories
{
	public interface ITournamentRepository
		: IGenericRepository<TournamentFilterDto, TournamentPreviewDto, TournamentDetailDto,
			NewTournamentDto, UpdateTournamentDto>,
			IAuthDataRepository<TournamentAuthDto>
	{
	}
}