﻿using System;
using System.Collections.Generic;

namespace OPCAIC.Messaging.Messages
{
	/// <summary>
	///     Message requesting the execution of a match between several bots.
	/// </summary>
	[Serializable]
	public class MatchExecutionRequest : WorkMessageBase
	{
		/// <summary>
		///     Bots participating in the match.
		/// </summary>
		public List<BotInfo> Bots { get; set; }

		/// <summary>
		///     Unique id of the match execution.
		/// </summary>
		public long ExecutionId { get; set; }

		/// <summary>
		///     Unique id of the match being executed
		/// </summary>
		public long MatchId { get; set; }
	}
}