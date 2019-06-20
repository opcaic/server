﻿using System;
using OPCAIC.Messaging.Messages;

namespace OPCAIC.Broker
{
	/// <summary>
	///   Item in a work queue to be scheduled on a worker
	/// </summary>
	internal class WorkItem : IComparable<WorkItem>
	{
		/// <summary>
		///   Timestamp when the workload was enqueued.
		/// </summary>
		public DateTime QueuedTime { get; set; }

		/// <summary>
		///   Workload message to be sent to the worker.
		/// </summary>
		public WorkMessageBase Payload { get; set; }


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