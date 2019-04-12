using NetMQ;
using OPCAIC.Messaging.Utils;

namespace OPCAIC.Messaging
{
	public class WorkerEntry
	{
		public WorkerEntry(string identity)
		{
			Identity = identity;
			PingTimer = new NetMQTimer(Defaults.HeartbeatInterval);
			Liveness = Defaults.Liveness;
		}

		public NetMQTimer PingTimer { get; }
		public int Liveness { get; set; }

		public string Identity { get; }
	}
}