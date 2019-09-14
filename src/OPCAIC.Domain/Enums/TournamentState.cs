namespace OPCAIC.Infrastructure.Enums
{
	/// <summary>
	///     States of tournaments' preparation.
	/// </summary>
	public enum TournamentState
	{
		/// <summary>
		///     Tournament was created.
		/// </summary>
		Created,

		/// <summary>
		///     Tournament was published and is accessible to users.
		/// </summary>
		Published,

		/// <summary>
		///     Tournament is currently being evaluated.
		/// </summary>
		Running,

		/// <summary>
		///     Manually stopped by administrator, no matches are generated.
		/// </summary>
		Stopped,

		/// <summary>
		///     All matches have been scheduled, waiting for the last ones to execute.
		/// </summary>
		WaitingForFinish,

		/// <summary>
		///     Tournament's evaluation ended.
		/// </summary>
		Finished
	}
}