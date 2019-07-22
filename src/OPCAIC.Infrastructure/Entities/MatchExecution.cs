using System;
using System.Collections.Generic;

namespace OPCAIC.Infrastructure.Entities
{
	public class MatchExecution : SoftDeletableEntity
	{
		public long MatchId { get; set; }
		public Match Match { get; set; }
		public long TournamentId { get; set; }

		/// <summary>
		///   Timestamp when this match was executed.
		/// </summary>
		public DateTime Executed { get; set; }

		/// <summary>
		///   Results of individual bots in this match.
		/// </summary>
		public virtual IList<SubmissionMatchResult> BotResults { get; set; }
	}
}