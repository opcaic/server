namespace OPCAIC.Messaging.Config
{
	public class BrokerConnectorConfig
	{
		public string Identity { get; set; }
		public string ListeningAddress { get; set; }
		public HeartbeatConfig HeartbeatConfig { get; set; }
		public string[] Games { get; set; }
	}
}
