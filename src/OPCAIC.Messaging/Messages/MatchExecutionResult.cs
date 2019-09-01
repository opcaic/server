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

		public SubTaskResult ExecutorResult { get; set; }

		public Dictionary<string, object> AdditionalData { get; set; } =
			new Dictionary<string, object>();
	}

	[Serializable]
	public class BotResult
	{
		public long SubmissionId { get; set; }

		public double Score { get; set; }

		public SubTaskResult CompilerResult { get; set; }

		public bool Crashed { get; set; }

		public Dictionary<string, object> AdditionalData { get; set; } =
			new Dictionary<string, object>();
	}
}