using System.Collections.Generic;
using OPCAIC.Application.Dtos.Matches;
using OPCAIC.Application.Dtos.Tournaments;
using OPCAIC.Domain.Enums;

namespace OPCAIC.Application.Interfaces
{
	/// <summary>
	///     Provides methods for generating matches for ongoing tournaments.
	/// </summary>
	public interface IOngoingMatchGenerator
	{
		/// <summary>
		///     Tournament format this generator is for.
		/// </summary>
		TournamentFormat Format { get; }

		/// <summary>
		///     Generates all matches for the tournament.
		/// </summary>
		/// <param name="tournament">The tournament for which to generate.</param>
		/// <param name="count">Number of matches to generate.</param>
		/// <returns></returns>
		List<NewMatchDto> Generate(TournamentOngoingGenerationDto tournament, int count);
	}
}