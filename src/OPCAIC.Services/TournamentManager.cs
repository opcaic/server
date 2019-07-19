using System.Collections.Generic;
using System.Linq;
using OPCAIC.Infrastructure.Entities;

namespace OPCAIC.Services
{
	public class TournamentManager
	{
		private readonly Dictionary<TournamentFormat, IMatchGenerator> matchGenerators;

		public TournamentManager(IEnumerable<IMatchGenerator> generators)
		{
			matchGenerators = generators.ToDictionary(g => g.Format);
		}
	}

	public class MatchGeneratorRegistry
	{
	}
}
