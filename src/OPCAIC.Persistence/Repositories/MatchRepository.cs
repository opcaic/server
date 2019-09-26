using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;
using OPCAIC.Application.Dtos.Matches;
using OPCAIC.Application.Interfaces.Repositories;
using OPCAIC.Application.Matches.Models;
using OPCAIC.Domain.Entities;

namespace OPCAIC.Persistence.Repositories
{
	public class MatchRepository
		: LookupRepository<Match, MatchDetailDto>, IMatchRepository
	{
		/// <inheritdoc />
		public MatchRepository(DataContext context, IMapper mapper)
			: base(context, mapper)
		{
		}

		/// <inheritdoc />
		public Task<MatchAuthDto> GetAuthorizationData(long id,
			CancellationToken cancellationToken = default)
		{
			return GetDtoByIdAsync<MatchAuthDto>(id, cancellationToken);
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