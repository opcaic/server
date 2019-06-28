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
				Tournament = tournament,
				Participants = new Submission[] {s}
			}).ToList();

			return (matches, true);
		}
	}
}
