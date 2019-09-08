using System;
using System.Collections.Generic;
using OPCAIC.Messaging.Messages;

namespace OPCAIC.Broker
{
	/// <summary>
	///     Item in a work queue to be scheduled on a worker
	/// </summary>
	public class WorkItem : IComparable<WorkItem>, ICloneable
	{
		/// <summary>
		///     Timestamp when the workload was enqueued.
		/// </summary>
		public DateTime QueuedTime { get; set; }

		/// <summary>
		///     Timestamp when the work item will expire.
		/// </summary>
		public DateTime ExpirationTime { get; set; }

		/// <summary>
		///     Workload message to be sent to the worker.
		/// </summary>
		public WorkMessageBase Payload { get; set; }

		/// <inheritdoc />
		public object Clone()
		{
			return new WorkItem
			{
				Payload = (WorkMessageBase)Payload.Clone(), QueuedTime = QueuedTime
			};
		}

		/// <inheritdoc />
		public int CompareTo(WorkItem other)
		{
			if (ReferenceEquals(this, other))
			{
				return 0;
			}

			if (ReferenceEquals(null, other))
			{
				return 1;
			}

			return QueuedTime.CompareTo(other.QueuedTime);
		}
	}
}