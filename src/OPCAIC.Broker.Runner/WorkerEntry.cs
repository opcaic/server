using System.Collections;
using OPCAIC.Messaging.Messages;

namespace OPCAIC.Broker.Runner
{
	public class WorkerEntry
	{
		public WorkerEntry(string identity, WorkerCapabilities capabilities)
		{
			Capabilities = capabilities;
			Identity = identity;
		}

		public string Identity { get; }

		public WorkerCapabilities Capabilities { get; }

		public ExecuteMatchMessage CurrentWorkItem { get; set; }
	}
}