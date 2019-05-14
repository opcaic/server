using System;

namespace OPCAIC.Messaging.Messages
{
	[Serializable]
	public class MatchExecutionResult : ReplyMessageBase
	{
		public MatchExecutionResult(int id) => Id = id;
	}
}
