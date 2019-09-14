namespace OPCAIC.Infrastructure.Enums
{
	/// <summary>
	///     Defines the format of the tournament (how matches are organized).
	/// </summary>
	public enum TournamentFormat
	{
		/// <summary>
		///     Unknown, should never occur.
		/// </summary>
		Unknown,

		/// <summary>
		///     Single-player tournament format.
		/// </summary>
		SinglePlayer,

		/// <summary>
		///     Single elimination brackets format.
		///     TODO: Single third place only?
		/// </summary>
		SingleElimination,

		/// <summary>
		///     Double elimination brackets format.
		/// </summary>
		DoubleElimination,

		/// <summary>
		///     Table format, each pair of competitors compete.
		/// </summary>
		Table,

		/// <summary>
		///     ELO tournament format. Competitors are matched by their ELO ratings.
		/// </summary>
		Elo
	}
}