namespace OPCAIC.Infrastructure.Enums
{
	/// <summary>
	///     State of job executed on a worker.
	/// </summary>
	public enum WorkerJobState
	{
		/// <summary>
		///     Execution is waiting in a queue to be scheduled.
		/// </summary>
		Waiting,

		/// <summary>
		///     Execution has been scheduled.
		/// </summary>
		Scheduled,

		/// <summary>
		///     Execution of the job has been cancelled.
		/// </summary>
		Cancelled,

		/// <summary>
		///     Execution of the job has finished.
		/// </summary>
		Finished,

		/// <summary>
		///     Execution is blocked by manual action from the administrator.
		/// </summary>
		Blocked
	}
}