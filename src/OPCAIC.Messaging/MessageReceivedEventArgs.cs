namespace OPCAIC.Messaging
{
	public class MessageReceivedEventArgs
	{
		public string Sender { get; set; }
		public object Message { get; set; }
	}
}