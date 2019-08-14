using OPCAIC.Messaging.Config;

namespace OPCAIC.Broker.Runner
{
	public class AppConfig
	{
		public BrokerConnectorConfig Broker { get; set; }
		public string[] Games { get; set; }
		public WorkerSetConfig WorkerSet { get; set; }
	}
}