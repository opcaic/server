using OPCAIC.Application.Dtos.Tournaments;
using OPCAIC.Application.Specifications;
using OPCAIC.Application.Tournaments.Commands;
using OPCAIC.Application.Tournaments.Models;
using OPCAIC.Domain.Entities;
using OPCAIC.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace OPCAIC.Application.Interfaces.Repositories
{
	public interface ITournamentRepository
		: IGenericRepository<TournamentDetailDto, CreateTournamentCommand, object>,
			IRepository<Tournament>
	{
		/// <summary>
		///     Gets tournaments for which to update the set of matches.
		/// </summary>
		/// <param name="lastUpdated"></param>
		/// <param name="cancellationToken"></param>
		/// <returns></returns>
		Task<List<TournamentBracketsGenerationDto>> GetBracketTournamentsForMatchGenerationAsync(DateTime lastUpdated,
			CancellationToken cancellationToken);
		Task<List<TournamentDeadlineGenerationDto>> GetDeadlineTournamentsForMatchGenerationAsync(CancellationToken cancellationToken);
		Task<List<TournamentOngoingGenerationDto>> GetOngoingTournamentsForMatchGenerationAsync(CancellationToken cancellationToken);

		Task<List<TournamentStateInfoDto>> GetTournamentsStateInfoAsync(
			IEnumerable<TournamentState> states, CancellationToken cancellationToken);
	}
}