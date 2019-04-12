namespace OPCAIC.Messaging.Commands
{
	public class Command<TItem>
	{
		private HandlerInfo<TItem> handler;
		private TItem arg;

		public Command(HandlerInfo<TItem> handler, TItem arg)
		{
			this.handler = handler;
			this.arg = arg;
		}

		public void Invoke()
		{
			handler.Handler(arg);
		}
	}
}