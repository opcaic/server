namespace OPCAIC.Domain.Enums
{
	/// <summary>
	///     The scope of the tournament.
	/// </summary>
	public enum TournamentScope
	{
		/// <summary>
		///     Unknown scope, should never occur.
		/// </summary>
		Unknown,

		/// <summary>
		///     Deadline scope. Competitors need to register before a fixed deadline.
		/// </summary>
		Deadline,

		/// <summary>
		///     Ongoing scope, matches are periodically generated until the end date, competitors may join
		///     anytime.
		/// </summary>
		Ongoing
	}
}