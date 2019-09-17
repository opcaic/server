namespace OPCAIC.Domain.Entities
{
	/// <summary>
	///     Represents an invitation of a (not necessarily existing) user to a tournament.
	/// </summary>
	public class TournamentInvitation : Entity
	{
		/// <summary>
		///     Id of the tournament to which the user is invited.
		/// </summary>
		public long TournamentId { get; set; }

		/// <summary>
		///     Tournament to which the user is invited.
		/// </summary>
		public virtual Tournament Tournament { get; set; }

		/// <summary>
		///     Email of the invited user.
		/// </summary>
		public string Email { get; set; }

		/// <summary>
		///     Id of the invited user. Can be null if there is no such user with given email.
		/// </summary>
		public long? UserId { get; set; }
		
		/// <summary>
		///     Invited user. Can be null if there is no such user with given email.
		/// </summary>
		public virtual User User { get; set; }
	}
}