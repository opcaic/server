using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using OPCAIC.Application.Dtos.Tournaments;
using OPCAIC.Domain.Enums;

namespace OPCAIC.Application.Interfaces.Repositories
{
	public interface ITournamentRepository
		: IGenericRepository<TournamentFilterDto, TournamentPreviewDto, TournamentDetailDto,
				NewTournamentDto, UpdateTournamentDto>,
			IAuthDataRepository<TournamentAuthDto>
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

		Task UpdateTournamentState(long id, TournamentStateUpdateDto dto,
			CancellationToken cancellationToken);
		Task UpdateTournamentState(long id, TournamentFinishedUpdateDto dto,
			CancellationToken cancellationToken);
		Task UpdateTournamentState(long id, TournamentStartedUpdateDto dto,
			CancellationToken cancellationToken);

		Task<List<TournamentReferenceDto>> GetTournamentsForFinishing(
			CancellationToken cancellationToken);

	}
}