using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using OPCAIC.Application.Dtos.TournamentParticipations;
using OPCAIC.Application.Interfaces.Repositories;
using OPCAIC.Domain.Entities;

namespace OPCAIC.Persistence.Repositories
{
	public class TournamentParticipationsRepository 
		: RepositoryBase<TournamentParticipation>,
			ITournamentParticipationsRepository
	{
		/// <inheritdoc />
		public TournamentParticipationsRepository(DataContext context, IMapper mapper) : base(context, mapper)
		{
		}

		/// <inheritdoc />
		public Task<bool> SetActiveSubmission(long tournamentId, long userId, UpdateTournamentParticipationDto dto,
			CancellationToken cancellationToken)
		{
			return UpdateFromDtoByQueryAsync(
				e => e.TournamentId == tournamentId && e.UserId == userId, dto, cancellationToken);
		}
	}
}