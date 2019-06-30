using System.Collections.Generic;
using OPCAIC.Utils;

namespace OPCAIC.Infrastructure.Entities
{
	public class Match : SoftDeletableEntity
	{
		/// <summary>
		///   Id of the tournament this match belongs to. In combination with <see cref="Entity.Id"/>
		///   uniquely identifies a match.
		/// </summary>
		public long TournamentId { get; set; }

		/// <summary>
		///   Tournament this match belongs to.
		/// </summary>
		public Tournament Tournament { get; set; }

		/// <summary>
		///   The <see cref="MatchState"/> this match is in.
		/// </summary>
		public MatchState MatchState { get; set; }

		/// <summary>
		///   List of execution attempts for this match.
		/// </summary>
		public virtual IList<MatchExecution> Executions { get; set; }

		/// <summary>
		///   List of participating submissions.
		/// </summary>
		public virtual IList<Submission> Participants { get; set; }

		/// <summary>
		///   Last execution of this match.
		/// </summary>
		public MatchExecution LastExecution => Executions?.ArgMaxOrDefault(e => e.Executed);
	}
}