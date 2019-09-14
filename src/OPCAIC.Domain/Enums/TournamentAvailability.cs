namespace OPCAIC.Domain.Enums
{
	/// <summary>
	///     Types of tournaments' availability.
	/// </summary>
	public enum TournamentAvailability
	{
		/// <summary>
		///     Unknown, should never occur.
		/// </summary>
		Unknown,

		/// <summary>
		///     Public tournament available to all users.
		/// </summary>
		Public,

		/// <summary>
		///     Private tournament available only to invited users.
		/// </summary>
		Private
	}
}