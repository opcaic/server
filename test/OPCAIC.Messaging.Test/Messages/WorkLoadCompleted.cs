using System;

namespace OPCAIC.Messaging.Test.Messages
{
	[Serializable]
	public class WorkLoadCompleted
	{
		public int WorkItem { get; set; }
	}
}