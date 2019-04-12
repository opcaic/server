using System;
using System.Collections.Generic;

namespace OPCAIC.Messaging.Messages
{
	[Serializable]
	public class WorkerConnectMessage
	{
		public WorkerCapabilities Capabilities { get; set; }
	}

	public class WorkerCapabilities
	{
		public List<string> SupportedLanguages { get; set; } = new List<string>();
		public List<string> SupportedGames { get; set; } = new List<string>();
	}
}
