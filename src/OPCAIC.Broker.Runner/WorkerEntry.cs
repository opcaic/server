using System.Collections;
using System.Collections.Generic;
using OPCAIC.Messaging.Messages;

namespace OPCAIC.Broker.Runner
{
	public class WorkerEntry
	{
		public WorkerEntry(string identity)
		{
			Identity = identity;
			TaskQueue = new Queue<ExecuteMatchMessage>();
		}

		public string Identity { get; }

		public Queue<ExecuteMatchMessage> TaskQueue { get; }

		public WorkerCapabilities Capabilities { get; set; }

		public ExecuteMatchMessage CurrentWorkItem { get; set; }
	}
}