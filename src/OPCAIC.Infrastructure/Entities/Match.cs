using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace OPCAIC.Infrastructure.Entities
{
	/// <summary>
	///     Represents a single match in a tournament.
	/// </summary>
	public class Match : SoftDeletableEntity
	{
		/// <summary>
		///     Index of the match in a tournament.
		/// </summary>
		public long Index { get; set; }

		/// <summary>
		///     Id of the tournament to which this match belongs.
		/// </summary>
		public long TournamentId { get; set; }

		/// <summary>
		///     Tournament this match belongs to.
		/// </summary>
		[Required]
		public virtual Tournament Tournament { get; set; }

		/// <summary>
		///     Reference to mapping table of matches and their participants.
		/// </summary>
		public virtual IList<SubmissionParticipation> Participations { get; set; }

		/// <summary>
		///     List of execution attempts for this match.
		/// </summary>
		public virtual IList<MatchExecution> Executions { get; set; }
	}
}