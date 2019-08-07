using System.Collections.Generic;
using OPCAIC.Infrastructure.Entities;
using OPCAIC.Infrastructure.Enums;

namespace OPCAIC.Services
{
	/// <summary>
	///     Contains methods for generating matches for tournaments.
	/// </summary>
	public interface IMatchGenerator
	{
		/// <summary>
		///     Tournament format this generator is for.
		/// </summary>
		TournamentFormat Format { get; }
		
		/// <summary>
		///     Generates new matches based on the current tournament state. The returned matches are
		///     *not* added to the queue as of yet. Also, in some cases it is not possible to generate all
		///     matches at once, so if the done flag is not set, then the generation may need to be rerun
		///     to get additional matches.
		/// </summary>
		/// <param name="tournament">The tournament to be updated.</param>
		/// <returns></returns>
		(IEnumerable<Match> matches, bool done) Generate(Tournament tournament);
	}
}