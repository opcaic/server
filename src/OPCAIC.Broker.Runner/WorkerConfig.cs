using OPCAIC.Messaging;
using OPCAIC.Messaging.Config;

namespace OPCAIC.Worker
{
	//TODO: remove this, as it is only for mocking 
	public class WorkerConfig
	{
		public string Identity { get; set; }
		public HeartbeatConfig HeartbeatConfig { get; set; }

		public string[] Supportedgames { get; set; }
	}
}
