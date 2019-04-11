using System;

namespace OPCAIC.Messaging.Messages
{
	[Serializable]
	public class WorkerConnectMessage
	{
		public WorkerConnectMessage()
		{
		}

		public WorkerConnectMessage(string message) => Message = message;

		public string Message { get; set; }
	}
}
