﻿using System;

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
		public Guid JobId { get; set; }

		/// <summary>
		///     Execution status of the job.
		/// </summary>
		public JobStatus JobStatus { get; set; }

		/// <summary>
		///     Message from <see cref="Exception"/> which caused the task to fail. Null if task finished successfully.
		/// </summary>
		public string Exception { get; set; }
	}
}