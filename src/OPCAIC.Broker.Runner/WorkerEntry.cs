using System.Collections;
using OPCAIC.Messaging.Messages;

namespace OPCAIC.Broker.Runner
{
	public class WorkerEntry
	{
		public WorkerEntry(string identity)
		{
			Identity = identity;
		}

		public string Identity { get; }

		public WorkerCapabilities Capabilities { get; set; }

		public ExecuteMatchMessage CurrentWorkItem { get; set; }
	}
}