using System;

namespace OPCAIC.Messaging
{
	[Serializable]
	public class WorkerConnectMessage
	{
		public WorkerConnectMessage()
		{
			
		}
		public WorkerConnectMessage(string message)
		{
			Message = message;
		}

		public string Message { get; set; }
	}

	[Serializable]
	public class WorkLoadMessage
	{
		public WorkLoadMessage(string work)
		{
			Work = work;
		}

		public string Work { get; set; }
	}

	[Serializable]
	public class WorkCompletedMessage
	{
		public WorkCompletedMessage(string work)
		{
			Work = work;
		}

		public string Work { get; set; }
	}

	[Serializable]
	public class PingMessage
	{
	}
}
