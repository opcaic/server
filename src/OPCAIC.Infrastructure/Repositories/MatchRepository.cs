using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;
using OPCAIC.Infrastructure.DbContexts;
using OPCAIC.Infrastructure.Dtos;
using OPCAIC.Infrastructure.Dtos.Matches;
using OPCAIC.Infrastructure.Entities;

namespace OPCAIC.Infrastructure.Repositories
{
	public class MatchRepository : Repository<Match>, IMatchRepository
	{
		/// <inheritdoc />
		public MatchRepository(DataContext context, IMapper mapper) : base(context, mapper)
		{
		}

		/// <inheritdoc />
		public IEnumerable<Match> AllMatchesFromTournament(long tournamentId)
		{
			return DbSet.Where(m => m.TournamentId == tournamentId).ToList();
		}

		/// <inheritdoc />
		public async Task<IEnumerable<Match>> AllMatchesFromTournamentAsync(long tournamentId,
			CancellationToken cancellationToken = default)
		{
			return await DbSet.Where(m => m.TournamentId == tournamentId).ToListAsync();
		}

		/// <inheritdoc />
		public Task<MatchDetailDto> FindByIdAsync(long id, CancellationToken cancellationToken)
		{
			return DbSet
				.Where(row => row.Id == id)
				.ProjectTo<MatchDetailDto>(Mapper.ConfigurationProvider)
				.SingleOrDefaultAsync(cancellationToken);
		}

		/// <inheritdoc />
		public async Task<ListDto<MatchDetailDto>> GetByFilterAsync(MatchFilterDto filter,
			CancellationToken cancellationToken)
		{
			var query = DbSet.Filter(filter);

			return new ListDto<MatchDetailDto>
			{
				List = await query
					.Skip(filter.Offset)
					.Take(filter.Count)
					.ProjectTo<MatchDetailDto>(Mapper.ConfigurationProvider)
					.ToListAsync(cancellationToken),
				Total = await query.CountAsync(cancellationToken)
			};
		}

		/// <inheritdoc />
		public Task<MatchExecutionStorageDto> FindExecutionForStorageAsync(long id,
			CancellationToken cancellationToken = default)
		{
			return Context.Set<MatchExecution>().Where(e => e.Id == id)
				.ProjectTo<MatchExecutionStorageDto>(Mapper.ConfigurationProvider)
				.SingleOrDefaultAsync(cancellationToken);
		}

		/// <inheritdoc />
		public Match Find(long matchId, long tournamentId)
		{
			return DbSet.Find(new {Id = matchId, TournamentId = tournamentId});
		}

		/// <inheritdoc />
		public Task<Match> FindAsync(long matchId, long tournamentId)
		{
			return DbSet.FindAsync(new {Id = matchId, TournamentId = tournamentId});
		}
	}
}