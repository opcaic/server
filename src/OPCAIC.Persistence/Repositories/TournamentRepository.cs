﻿using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;
using OPCAIC.Application.Dtos.Tournaments;
using OPCAIC.Application.Interfaces.Repositories;
using OPCAIC.Application.Tournaments.Commands;
using OPCAIC.Application.Tournaments.Models;
using OPCAIC.Domain.Entities;
using OPCAIC.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace OPCAIC.Persistence.Repositories
{
	public class TournamentRepository
		: GenericRepository<Tournament, TournamentDetailDto, CreateTournamentCommand, object>,
			ITournamentRepository
	{

		/// <inheritdoc />
		public TournamentRepository(DataContext context, IMapper mapper)
			: base(context, mapper)
		{
		}

		/// <inheritdoc />
		public Task<TournamentOngoingGenerationDto> GetForOngoingMatchGenerationAsync(long id,
			CancellationToken cancellationToken)
		{
			return GetDtoByIdAsync<TournamentOngoingGenerationDto>(id, cancellationToken);
		}

		/// <inheritdoc />
		public Task<List<TournamentStateInfoDto>> GetTournamentsStateInfoAsync(
			IEnumerable<TournamentState> states, CancellationToken cancellationToken)
		{
			return Query(t => states.Contains(t.State))
				.ProjectTo<TournamentStateInfoDto>(Mapper.ConfigurationProvider)
				.ToListAsync(cancellationToken);
		}

		public Task<List<TournamentDeadlineGenerationDto>> GetDeadlineTournamentsForMatchGenerationAsync(CancellationToken cancellationToken)
		{
			return Query(t => t.State == TournamentState.Running)
				.Where(t => t.Scope == TournamentScope.Deadline)
				.Where(t => t.Format != TournamentFormat.DoubleElimination && t.Format != TournamentFormat.SingleElimination) // these have their own logic
				.ProjectTo<TournamentDeadlineGenerationDto>(Mapper.ConfigurationProvider)
				.ToListAsync(cancellationToken);
		}

		public Task<List<TournamentOngoingGenerationDto>> GetOngoingTournamentsForMatchGenerationAsync(CancellationToken cancellationToken)
		{
			var q = Query(t => t.State == TournamentState.Running)
				.Where(t => t.Scope == TournamentScope.Ongoing);
			return q.ProjectTo<TournamentOngoingGenerationDto>(Mapper.ConfigurationProvider)
				.ToListAsync(cancellationToken);
		}

		/// <inheritdoc />
		public Task<List<TournamentBracketsGenerationDto>> GetBracketTournamentsForMatchGenerationAsync(
			DateTime lastUpdated, CancellationToken cancellationToken)
		{
			return Query(t => t.State == TournamentState.Running)
				.Where(t => t.Format == TournamentFormat.DoubleElimination || t.Format == TournamentFormat.SingleElimination)
				.Where(t => !t.Matches.Any() || // only those, which have changed
					t.Matches.Any(m
						=> m.Executions.Any(e => e.Executed.HasValue && e.Executed >= lastUpdated)))
				.ProjectTo<TournamentBracketsGenerationDto>(Mapper.ConfigurationProvider)
				.ToListAsync(cancellationToken);
		}
	}
}