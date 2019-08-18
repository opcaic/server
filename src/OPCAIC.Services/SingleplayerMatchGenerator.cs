using System.Collections.Generic;
using System.Linq;
using OPCAIC.Infrastructure.Entities;
using OPCAIC.Infrastructure.Enums;

namespace OPCAIC.Services
{
	/// <summary>
	///     Match generator for the single player games (generates a "match" per submission).
	/// </summary>
	public class SinglePlayerMatchGenerator : IMatchGenerator
	{
		/// <inheritdoc />
		public TournamentFormat Format => TournamentFormat.SinglePlayer;

		/// <inheritdoc />
		public (IEnumerable<Match> matches, bool done) Generate(Tournament tournament)
		{
			var i = 0;
			var matches = tournament.GetActiveSubmissions().Select(s => new Match
			{
				Index = i++,
				Tournament = tournament,
				Participations = new[] {new SubmissionParticipation {Submission = s}}
			}).ToList();

			return (matches, true);
		}
	}
}