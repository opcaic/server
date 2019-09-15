using System.Collections.Generic;
using OPCAIC.Application.Dtos.Matches;
using OPCAIC.Application.Dtos.Tournaments;
using OPCAIC.Domain.Enums;

namespace OPCAIC.Application.Interfaces.MatchGeneration
{
	/// <summary>
	///     Provides methods for generating matches for deadline scoped tournaments.
	/// </summary>
	public interface IDeadlineMatchGenerator
	{
		/// <summary>
		///     Tournament format this generator is for.
		/// </summary>
		TournamentFormat Format { get; }

		/// <summary>
		///     Generates all matches for the tournament.
		/// </summary>
		/// <param name="tournament">The tournament for which to generate.</param>
		/// <returns></returns>
		List<NewMatchDto> Generate(TournamentDeadlineGenerationDto tournament);
	}
}