using System.Collections.Generic;
using System.Linq;
using OPCAIC.Infrastructure.Entities;

namespace OPCAIC.Services
{
	public class TableMatchGenerator : IDeadlineMatchGenerator
	{
		/// <inheritdoc />
		public (IEnumerable<Match> matches, bool done) Generate(Tournament tournament)
		{
			var submissions = tournament.GetActiveSubmissions().ToList();

			IList<Match> matches = new List<Match>(submissions.Count * (submissions.Count - 1) / 2);

			for (var i = 0; i < submissions.Count; i++)
			for (var j = i + 1; j < submissions.Count; j++)
			{
				matches.Add(new Match
				{
					MatchState = MatchState.Waiting, 
					Tournament = tournament,
					Participants = new[] {submissions[i], submissions[j]}
				});
			}

			return (matches, true);
		}
	}
}
