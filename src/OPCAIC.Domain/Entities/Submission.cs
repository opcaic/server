using System.Collections.Generic;
using OPCAIC.Domain.Enums;

namespace OPCAIC.Domain.Entities
{
	/// <summary>
	///     Represents a bot submission to a tournament.
	/// </summary>
	public class Submission : Entity
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
		///     Total score obtained by submission in a tournament. The meaning of this value is dependent on tournament format.
		/// </summary>
		public double Score { get; set; }

		/// <summary>
		///     Id of the tournament this submission has been posted to.
		/// </summary>
		public long TournamentId { get; set; }

		/// <summary>
		///     Tournament this submission has been posted to.
		/// </summary>
		public virtual Tournament Tournament { get; set; }

		/// <summary>
		///     Link to the participation of this submission in the tournament.
		/// </summary>
		public virtual TournamentParticipation TournamentParticipation { get; set; }

		/// <summary>
		///     Reference to mapping table of matches and their participants.
		/// </summary>
		public virtual IList<SubmissionParticipation> Participations { get; set; }

		/// <summary>
		///     All validations of this submission.
		/// </summary>
		public virtual IList<SubmissionValidation> Validations { get; set; }

		/// <summary>
		///     Last validation which determines the validation state of this submission.
		/// </summary>
		public virtual SubmissionValidation LastValidation { get; set; }

		/// <summary>
		///     Resulting state of the validation process.
		/// </summary>
		public SubmissionValidationState ValidationState { get; set; }

	}
}