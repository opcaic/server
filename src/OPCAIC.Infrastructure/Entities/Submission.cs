using System.Collections.Generic;

namespace OPCAIC.Infrastructure.Entities
{
	/// <summary>
	///     Represents a bot submission to a tournament.
	/// </summary>
	public class Submission : SoftDeletableEntity
	{
		/// <summary>
		///     Id of the user who created this submission.
		/// </summary>
		public long AuthorId { get; set; }

		/// <summary>
		///     Author of this submission.
		/// </summary>
		public virtual User Author { get; set; }

		/// <summary>
		///     Whether this submission is active and should participate in tournament matches.
		/// </summary>
		public bool IsActive { get; set; }

		/// <summary>
		///     Id of the tournament this submission has been posted to.
		/// </summary>
		public long TournamentId { get; set; }

		/// <summary>
		///     Tournament this submission has been posted to.
		/// </summary>
		public virtual Tournament Tournament { get; set; }

		/// <summary>
		///     Reference to mapping table of matches and their participants.
		/// </summary>
		public virtual IList<SubmissionParticipation> Participations { get; set; }

		/// <summary>
		///     All validations of this submission.
		/// </summary>
		public virtual IList<SubmissionValidation> Validations { get; set; }
	}
}