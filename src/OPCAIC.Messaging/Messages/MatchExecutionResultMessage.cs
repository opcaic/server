using System;

namespace OPCAIC.Messaging.Messages
{
	[Serializable]
	public class MatchExecutionResultMessage
	{
		public MatchExecutionResultMessage(int work) => Work = work;

		public int Work { get; set; }
	}
}
