using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;
using OPCAIC.Infrastructure.DbContexts;
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

		public Task CreateMatchesAsync(List<NewMatchDto> matches,
			CancellationToken cancellationToken)
		{
			var entities = Mapper.Map<List<Match>>(matches);

			for (var i = 0; i < entities.Count; i++)
			{
				// enforce correct order
				var e = entities[i];
				e.Participations = new List<SubmissionParticipation>();
				foreach (var id in matches[i].Submissions)
				{
					e.Participations.Add(new SubmissionParticipation
					{
						SubmissionId = id, Order = e.Participations.Count
					});
				}

				// make sure there is at least one execution
				e.Executions = new List<MatchExecution>
				{
					new MatchExecution {JobId = Guid.NewGuid()}
				};
			}

			DbSet.AddRange(entities);
			return SaveChangesAsync(cancellationToken);
		}

		/// <inheritdoc />
		public Task<List<MatchDetailDto>> AllMatchesFromTournamentAsync(long tournamentId, CancellationToken cancellationToken)
		{
			return Query(m => m.TournamentId == tournamentId)
				.ProjectTo<MatchDetailDto>(Mapper.ConfigurationProvider)
				.ToListAsync(cancellationToken);
		}
	}
}