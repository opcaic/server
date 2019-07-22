namespace OPCAIC.Infrastructure.Enums
{
	/// <summary>
	///   State of a match.
	/// </summary>
	public enum MatchState
	{
		/// <summary>
		///   Unknown state, should never occur.
		/// </summary>
		Unknown,

		/// <summary>
		///   Match is blocked by manual action from the administrator.
		/// </summary>
		Blocked,

		/// <summary>
		///   Match is waiting in a queue to be scheduled.
		/// </summary>
		Waiting,

		/// <summary>
		///   Match has been scheduled for execution.
		/// </summary>
		Scheduled,

		/// <summary>
		///   Execution of the match has failed.
		/// </summary>
		Failed,

		/// <summary>
		///   Match has been successfully executed.
		/// </summary>
		Finished
	}
}
