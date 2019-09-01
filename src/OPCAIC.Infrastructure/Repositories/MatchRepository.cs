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
using OPCAIC.Infrastructure.Dtos.MatchExecutions;
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
			return await DbSet.Where(m => m.TournamentId == tournamentId)
				.ToListAsync(cancellationToken);
		}

		/// <inheritdoc />
		public Task<MatchAuthDto> GetAuthorizationData(long id,
			CancellationToken cancellationToken = default)
		{
			return DbSet.Where(m => m.Id == id)
				.Select(m => new MatchAuthDto
				{
					TournamentOwnerId = m.Tournament.OwnerId,
					ParticipantsIds =
						m.Participations.Select(p => p.Submission.AuthorId).ToArray(),
					TournamentManagersIds =
						m.Tournament.Managers.Select(u => u.UserId).ToArray()
				}).SingleOrDefaultAsync(cancellationToken);
		}
	}
}