using System.Collections.Generic;
using OPCAIC.Infrastructure.Dtos.Matches;
using OPCAIC.Infrastructure.Dtos.Tournaments;
using OPCAIC.Infrastructure.Enums;

namespace OPCAIC.Services
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