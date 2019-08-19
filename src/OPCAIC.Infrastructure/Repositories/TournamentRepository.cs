using AutoMapper;
using OPCAIC.Infrastructure.DbContexts;
using OPCAIC.Infrastructure.Dtos.Tournaments;
using OPCAIC.Infrastructure.Entities;

namespace OPCAIC.Infrastructure.Repositories
{
	public class TournamentRepository
		: GenericRepository<Tournament, TournamentFilterDto, TournamentPreviewDto,
				TournamentDetailDto, NewTournamentDto, UpdateTournamentDto>,
			ITournamentRepository
	{
		/// <inheritdoc />
		public TournamentRepository(DataContext context, IMapper mapper)
			: base(context, mapper, QueryableExtensions.Filter)
		{
		}

		/// <inheritdoc />
		public async Task<bool> CheckTournamentExists(long id)
		{
			return Context.Set<Tournament>().Any(t => t.Id == id);
		}
	}
}