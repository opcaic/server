using System.Collections.Generic;
using System.Linq;
using OPCAIC.Infrastructure.Entities;
using OPCAIC.Infrastructure.Enums;

namespace OPCAIC.Services
{
	/// <summary>
	///   Match generator for the single player games (generates a "match" per submission).
	/// </summary>
	public class SinglePlayerMatchGenerator : IMatchGenerator
	{
		/// <inheritdoc />
		public TournamentFormat Format => TournamentFormat.SinglePlayer;

		/// <inheritdoc />
		public (IEnumerable<Match> matches, bool done) Generate(Tournament tournament)
		{
			var matches = tournament.GetActiveSubmissions().Select(s => new Match
			{
				MatchState = MatchState.Waiting, // no dependency
				Tournament = tournament,
				Participants = new[] {s}
			}).ToList();

			return (matches, true);
		}
	}
}
