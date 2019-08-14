using System;
using System.Collections.Generic;

namespace OPCAIC.Messaging.Messages
{
	/// <summary>
	///     Message with result of a match execution
	/// </summary>
	[Serializable]
	public class MatchExecutionResult : ReplyMessageBase
	{
		public BotResult[] BotResults { get; set; }

		public SubTaskResult ExecutionResult { get; set; }

		public Dictionary<string, object> AdditionalData { get; set; } =
			new Dictionary<string, object>();
	}

	public class BotResult
	{
		public double Score { get; set; }

		public SubTaskResult CompilationResult { get; set; }

		public bool Crashed { get; set; }

		public Dictionary<string, object> AdditionalData { get; set; } =
			new Dictionary<string, object>();
	}
}