using System.Collections.Generic;
using OPCAIC.Messaging.Messages;

namespace OPCAIC.Broker.Runner
{
	internal class WorkerEntry
	{
		public WorkerEntry(string identity)
		{
			Identity = identity;
		}

		public string Identity { get; }

		public WorkerCapabilities Capabilities { get; set; }

		public WorkItem CurrentWorkItem { get; set; }
	}
}
