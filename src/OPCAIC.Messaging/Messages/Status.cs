namespace OPCAIC.Messaging.Messages
{
	/// <summary>
	///   Result status of the job sent from broker to worker for execution.
	/// </summary>
	public enum Status
	{
		/// <summary>
		///   Unknown status.
		/// </summary>
		Unknown,

		/// <summary>
		///   Job completed successfully.
		/// </summary>
		Ok,

		/// <summary>
		///   Job completed with errors.
		/// </summary>
		Error
	}
}
