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
	public class MatchRepository
		: LookupRepository<Match, MatchFilterDto, MatchDetailDto, MatchDetailDto>, IMatchRepository
	{
		/// <inheritdoc />
		public MatchRepository(DataContext context, IMapper mapper)
			: base(context, mapper, QueryableExtensions.Filter)
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
		public Task<MatchExecutionStorageDto> FindExecutionForStorageAsync(long id,
			CancellationToken cancellationToken = default)
		{
			return GetDbSet<MatchExecution>().Where(e => e.Id == id)
				.ProjectTo<MatchExecutionStorageDto>(Mapper.ConfigurationProvider)
				.SingleOrDefaultAsync(cancellationToken);
		}
	}
}