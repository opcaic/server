﻿namespace OPCAIC.Messaging
{
	public class WorkerConnectorConfig
	{
		public string Identity { get; set; }
		public string BrokerAddress { get; set; }
		public HeartbeatConfig HeartbeatConfig { get; set; }
	}
}
