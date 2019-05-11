namespace OPCAIC.Messaging
{
	public class BrokerConnectorConfig
	{
		public string Identity { get; set; }
		public string ListeningAddress { get; set; }
		public HeartbeatConfig HeartbeatConfig { get; set; }
		public string[] Games { get; set; }
	}
}
