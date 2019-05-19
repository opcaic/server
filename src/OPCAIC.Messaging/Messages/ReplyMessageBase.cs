using System;

namespace OPCAIC.Messaging.Messages
{
	/// <summary>
	///   Base message for reply messages sent from worker to Broker
	/// </summary>
	[Serializable]
	public class ReplyMessageBase
	{
		/// <summary>
		///   Unique identifier of the job.
		/// </summary>
		public int Id { get; set; }

		/// <summary>
		///   Execution status of the job.
		/// </summary>
		public Status Status { get; set; }
	}
}
