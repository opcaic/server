using System;
using OPCAIC.Messaging.Config;

namespace OPCAIC.Messaging.Messages
{
	[Serializable]
	public class SetHeartbeatMessage
	{
		public HeartbeatConfig HeartbeatConfig { get; set; }
	}

	[Serializable]
	public class CancelTaskMessage
	{
	}
}
