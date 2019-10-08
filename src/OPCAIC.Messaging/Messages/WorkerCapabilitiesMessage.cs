using System;
using System.Collections.Generic;

namespace OPCAIC.Messaging.Messages
{
	/// <summary>
	///     Message sent by worker to broker upon connection.
	/// </summary>
	[Serializable]
	public class WorkerCapabilitiesMessage
	{
		/// <summary>
		///     Capabilities of the worker.
		/// </summary>
		public WorkerCapabilities Capabilities { get; set; }
	}

	/// <summary>
	///     Class representing the set of capabilities of a worker.
	/// </summary>
	public class WorkerCapabilities
	{
		/// <summary>
		///     Set of games the worker is able to run.
		/// </summary>
		public List<string> SupportedGames { get; set; } = new List<string>();
	}
}