using System;
using OPCAIC.Messaging.Config;

namespace OPCAIC.Messaging.Messages
{
	[Serializable]
	public class SetConfigMessage
	{
		public HeartbeatConfig HeartbeatConfig { get; set; }

		public TimeSpan ReportPeriod { get; set; }
	}

	[Serializable]
	public class CancelTaskMessage
	{
	}
}