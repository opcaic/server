﻿using System.Collections.Generic;
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
	public class MatchRepository : Repository<Match>, IMatchRepository
	{
		/// <inheritdoc />
		public MatchRepository(DataContext context, IMapper mapper) : base(context, mapper)
		{
		}

		/// <inheritdoc />
		public Match Find(long matchId, long tournamentId)
			=> DbSet.Find(new {Id = matchId, TournamentId = tournamentId});

		/// <inheritdoc />
		public Task<Match> FindAsync(long matchId, long tournamentId)
			=> DbSet.FindAsync(new {Id = matchId, TournamentId = tournamentId});

		/// <inheritdoc />
		public IEnumerable<Match> AllMatchesFromTournament(long tournamentId)
			=> DbSet.Where(m => m.TournamentId == tournamentId).ToList();

		/// <inheritdoc />
		public async Task<IEnumerable<Match>> AllMatchesFromTournamentAsync(long tournamentId,
			CancellationToken cancellationToken = default)
			=> await DbSet.Where(m => m.TournamentId == tournamentId).ToListAsync();

		/// <inheritdoc />
		public Task<MatchExecutionStorageDto> FindExecutionForStorageAsync(long id,
			CancellationToken cancellationToken = default)
			=> Context.Set<MatchExecution>().Where(e => e.Id == id)
				.ProjectTo<MatchExecutionStorageDto>(Mapper.ConfigurationProvider)
				.SingleOrDefaultAsync(cancellationToken);
	}
}