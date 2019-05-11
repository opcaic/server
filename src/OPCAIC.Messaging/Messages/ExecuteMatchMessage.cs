using System;

namespace OPCAIC.Messaging.Messages
{
	[Serializable]
	public class ExecuteMatchMessage
	{
		public string Game { get; set; }

		public int Id { get; set; }

//		public List<BotInfo> Bots { get; set; }
	}
}
