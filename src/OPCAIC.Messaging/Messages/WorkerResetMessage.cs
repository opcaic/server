using System;
using OPCAIC.Messaging.Config;

namespace OPCAIC.Messaging.Messages
{
	[Serializable]
	public class WorkerResetMessage
	{
		public HeartbeatConfig HeartbeatConfig { get; set; }
	}
}
