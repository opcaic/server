namespace OPCAIC.Messaging
{
	/// <summary>
	///     Wrapper around the sender identity and received message for the broker connector.
	/// </summary>
	public class ReceivedMessage
	{
		public ReceivedMessage(string sender, object payload)
		{
			Sender = sender;
			Payload = payload;
		}

		/// <summary>
		///     Sender identity.
		/// </summary>
		public string Sender { get; }

		/// <summary>
		///     Message payload.
		/// </summary>
		public object Payload { get; }
	}
}