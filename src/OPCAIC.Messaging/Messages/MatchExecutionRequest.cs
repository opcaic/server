using System;
using System.Collections.Generic;

namespace OPCAIC.Messaging.Messages
{
	[Serializable]
	public class MatchExecutionRequest : WorkMessageBase
	{
		public List<BotInfo> Bots { get; set; }
	}
}
