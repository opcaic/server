using System.Collections.Generic;
using System.Linq;
using OPCAIC.Infrastructure.Entities;

namespace OPCAIC.Services
{
	public class SinglePlayerMatchGenerator : IDeadlineMatchGenerator
	{
		/// <inheritdoc />
		public (IEnumerable<Match> matches, bool done) Generate(Tournament tournament)
		{
			var matches = tournament.GetActiveSubmissions().Select(s => new Match()
			{
				MatchState = MatchState.Waiting, // no dependency
				Tournament = tournament,
				Participants = new[] {s}
			}).ToList();

			return (matches, true);
		}
	}
}
