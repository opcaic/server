namespace OPCAIC.Messaging.Messages
{
	public class ReceivedMessage
	{
		public ReceivedMessage(string sender, object payload)
		{
			Sender = sender;
			Payload = payload;
		}

		public string Sender { get; }
		public object Payload { get; }
	}
}