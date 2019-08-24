using OPCAIC.Messaging.Messages;

namespace OPCAIC.Broker
{
	/// <summary>
	///     Entry for a worker.
	/// </summary>
	internal class WorkerEntry
	{
		public WorkerEntry(string identity)
		{
			Identity = identity;
		}

		/// <summary>
		///     Identity of the worker.
		/// </summary>
		public string Identity { get; }

		/// <summary>
		///     Worker's capabilities.
		/// </summary>
		public WorkerCapabilities Capabilities { get; set; }

		/// <summary>
		///     Currently executing item on the worker.
		/// </summary>
		public WorkItemDto CurrentWorkItem { get; set; }
	}
}