using System;

namespace OPCAIC.Messaging.Messages
{
	[Serializable]
	public class RefuseMessage
	{
		public MatchExecutionRequest WorkItem { get; set; }

		public TaskRefusalReason Reason { get; set; }
	}
}
