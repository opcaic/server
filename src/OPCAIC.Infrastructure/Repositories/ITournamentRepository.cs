using System.Threading;
using System.Threading.Tasks;
using OPCAIC.Infrastructure.Dtos.Tournaments;

namespace OPCAIC.Infrastructure.Repositories
{
	public interface ITournamentRepository
		: IGenericRepository<TournamentFilterDto, TournamentPreviewDto, TournamentDetailDto,
			NewTournamentDto, UpdateTournamentDto>
	{
		Task<TournamentAuthorizationDto> GetTournamentAuthorizationData(long id, CancellationToken cancellationToken = default);
	}
}