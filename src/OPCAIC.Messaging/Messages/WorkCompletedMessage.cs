using System;

namespace OPCAIC.Messaging.Messages
{
	[Serializable]
	public class WorkCompletedMessage
	{
		public WorkCompletedMessage(int work) => Work = work;

		public int Work { get; set; }
	}
}
