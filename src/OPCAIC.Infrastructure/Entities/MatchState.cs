namespace OPCAIC.Infrastructure.Entities
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
		///   Match cannot be queued, because something is not ready.
		/// </summary>
		WaitingForDependency,

		/// <summary>
		///   Match is waiting in a queue to be scheduled.
		/// </summary>
		Queued,

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
