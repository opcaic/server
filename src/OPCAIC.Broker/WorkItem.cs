using System;
using OPCAIC.Messaging.Messages;

namespace OPCAIC.Broker
{
	internal class WorkItem : IComparable<WorkItem>
	{
		public DateTime QueuedTime { get; set; }
		public WorkMessageBase Payload { get; set; }

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

			// order by time
			return QueuedTime.CompareTo(other.QueuedTime);
		}
	}
}