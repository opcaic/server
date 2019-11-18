using System;
using System.Collections.Generic;

namespace OPCAIC.Domain.Entities
{
	/// <summary>
	///     Represents a participation of a user in a tournament.
	/// </summary>
	public class TournamentParticipation : EntityBase, IChangeTrackable
	{
		/// <summary>
		///     Tournament in which user participates.
		/// </summary>
		public virtual Tournament Tournament { get; set; }

		/// <summary>
		///     Id of the tournament in which user participates.
		/// </summary>
		public long TournamentId { get; set; }

		/// <summary>
		///     User who participates in the tournament.
		/// </summary>
		public virtual User User { get; set; }

		/// <summary>
		///     Id of the user participating in the tournament.
		/// </summary>
		public long? UserId { get; set; }

		/// <summary>
		///     User's active submission in the tournament (which should be used for matches).
		/// </summary>
		public virtual Submission ActiveSubmission { get; set; }

		/// <summary>
		///     All submissions submitted into the tournament by the user.
		/// </summary>
		public virtual IList<Submission> Submissions { get; set; }

		/// <summary>
		///     Id of the user's submission to be used in the matches.
		/// </summary>
		public long? ActiveSubmissionId { get; set; }

		/// <summary>
		///     Timestamp when the user joined the tournament.
		/// </summary>
		public DateTime Created { get; set; }

		/// <inheritdoc />
		public DateTime Updated { get; set; }
	}
}