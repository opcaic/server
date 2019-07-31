namespace OPCAIC.Messaging.Messages
{
	/// <summary>
	///     Result status of the job sent from broker to worker for execution.
	/// </summary>
	public enum JobStatus
	{
		/// <summary>
		///     Unknown status.
		/// </summary>
		Unknown,

		/// <summary>
		///     Job completed successfully.
		/// </summary>
		Ok,

		/// <summary>
		///     Job was cancelled.
		/// </summary>
		Canceled,

		/// <summary>
		///     Job was forcibly terminated by worker after timeout period expired.
		/// </summary>
		Timeout,

		/// <summary>
		///     Job completed with errors.
		/// </summary>
		Error,
	}
}