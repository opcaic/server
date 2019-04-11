using System;

namespace OPCAIC.Messaging.Messages
{
	[Serializable]
	public class WorkLoadMessage
	{
		public WorkLoadMessage(int work) => Work = work;

		public int Work { get; set; }
	}
}
