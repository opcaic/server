
namespace OPCAIC.Infrastructure.Entities
{
	/// <summary>
	///     Join entity for matching submissions to matches in which they participate (many-to-many relationship.
	/// </summary>
	public class SubmissionParticipation
	{
		/// <summary>
		///     Id of the match in which <see cref="Submission" /> participates.
		/// </summary>
		public long MatchId { get; set; }

		/// <summary>
		///     Match in which <see cref="Submission" /> participates.
		/// </summary>
		public virtual Match Match { get; set; }

		/// <summary>
		///     Id of the submission which participates in the <see cref="Match" />.
		/// </summary>
		public long SubmissionId { get; set; }

		/// <summary>
		///     Submission which participates in the <see cref="Match" />.
		/// </summary>
		public virtual Submission Submission { get; set; }

		/// <summary>
		///     Order of the submission in the match. This makes sure the submissions are always in the same order.
		/// </summary>
		public int Order { get; set; }
	}
}