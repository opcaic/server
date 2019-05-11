using OPCAIC.Messaging;

namespace OPCAIC.Worker
{
	public class WorkerConfig
	{
		public string Identity { get; set; }
		public HeartbeatConfig HeartbeatConfig { get; set; }
		public string[] Supportedgames { get; set; }
	}
}
