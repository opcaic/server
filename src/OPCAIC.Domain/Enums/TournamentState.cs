namespace OPCAIC.Domain.Enums
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
		///     All matches have been scheduled, waiting for the last ones to execute.
		/// </summary>
		WaitingForFinish,

		/// <summary>
		///     Tournament's evaluation ended.
		/// </summary>
		Finished,

		/// <summary>
		///     Tournament's evaluation was manually paused by administrator, no matches are executed until unpausing.
		/// </summary>
		Paused
	}
}