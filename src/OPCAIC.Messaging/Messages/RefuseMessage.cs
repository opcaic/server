using System;

namespace OPCAIC.Messaging.Messages
{
	[Serializable]
	public class RefuseMessage
	{
		public ExecuteMatchMessage WorkItem { get; set; }

		public TaskRefusalReason Reason { get; set; }
	}
}
