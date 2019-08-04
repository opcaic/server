namespace OPCAIC.Infrastructure.Enums
{
	/// <summary>
	///     States of tournaments' preparation.
	/// </summary>
	public enum TournamentState
	{
		/// <summary>
		///     Unknown, should never occur.
		/// </summary>
		Unknown,

		/// <summary>
		///     Tournament was created.
		/// </summary>
		Created,

		/// <summary>
		///     Tournament was published and is accessible to users.
		/// </summary>
		Published,

		/// <summary>
		///     Tournament is ready to run.
		/// </summary>
		ReadyToRun,

		/// <summary>
		///     Tournament is currently being evaluated.
		/// </summary>
		Running,

		/// <summary>
		///     Tournament's evaluation ended.
		/// </summary>
		Finished
	}
}