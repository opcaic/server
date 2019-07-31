using System;

namespace OPCAIC.Messaging.Messages
{
	/// <summary>
	///     Base message for reply messages sent from worker to Broker
	/// </summary>
	[Serializable]
	public class ReplyMessageBase
	{
		/// <summary>
		///     Unique identifier of the job.
		/// </summary>
		public Guid Id { get; set; }

		/// <summary>
		///     Execution status of the job.
		/// </summary>
		public JobStatus JobStatus { get; set; }

		/// <summary>
		///     Exception which led to an error status of the job.
		/// </summary>
		public Exception Exception { get; set; }
	}
}