using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;
using OPCAIC.Infrastructure.DbContexts;
using OPCAIC.Infrastructure.Dtos;
using OPCAIC.Infrastructure.Entities;

namespace OPCAIC.Infrastructure.Repositories
{
	public class TournamentRepository : Repository<Tournament>, ITournamentRepository
	{
		/// <inheritdoc />
		public TournamentRepository(DataContext context, IMapper mapper) : base(context, mapper)
		{
		}

		/// <inheritdoc />
		public Task<TournamentInfoDto[]> GetAllTournamentsInfo(
			CancellationToken cancellationToken = default)
			=> DbSet.ProjectTo<TournamentInfoDto>(Mapper.ConfigurationProvider)
				.ToArrayAsync(cancellationToken);

		/// <inheritdoc />
		public Task<TournamentInfoDto> GetAllTournamentInfo(long id,
			CancellationToken cancellationToken = default)
			=> DbSet.Where(t => t.Id == id)
				.ProjectTo<TournamentInfoDto>(Mapper.ConfigurationProvider)
				.SingleOrDefaultAsync(cancellationToken);

		/// <inheritdoc />
		public async Task UpdateTournament(TournamentInfoDto tournament,
			CancellationToken cancellationToken = default)
		{
			var existing = await DbSet.FindAsync(tournament.Id) ?? new Tournament();

//			Context.Entry(existing).State = EntityState.Modified;
			Context.Entry(existing).CurrentValues.SetValues(tournament);

			await SaveChangesAsync();
		} 
	}
}