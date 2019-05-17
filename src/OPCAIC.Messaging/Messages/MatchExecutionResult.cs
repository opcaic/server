using System;

namespace OPCAIC.Messaging.Messages
{
	/// <summary>
	///   Message with result of a match execution
	/// </summary>
	[Serializable]
	public class MatchExecutionResult : ReplyMessageBase
	{
		public MatchExecutionResult(int id) => Id = id;
	}
}
