namespace OPCAIC.Messaging.Messages
{
	public class OutboundMessage<T>
	{
		internal OutboundMessage(T payload, string receiver)
		{
			Payload = payload;
			Receiver = receiver;
		}

		public string Receiver { get; }

		public T Payload { get; }
	}
}
