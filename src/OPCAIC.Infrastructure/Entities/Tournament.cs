using OPCAIC.Infrastructure.Entities;
using System.Collections.Generic;

namespace OPCAIC.Infrastructure.Entities
{
	public class Tournament : Entity
	{
		public string Name { get; set; }

		public string Description { get; set; }

		public virtual IList<Submission> Submissions { get; set; }

		public TournamentFormat Format { get; set; }

		public TournamentScope Scope { get; set; }

		public TournamentRankingStrategy RankingStrategy { get; set; }
	}
}
