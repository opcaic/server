namespace OPCAIC.Messaging.Commands
{
	public class Command<TItem>
	{
		private readonly TItem arg;
		private readonly HandlerInfo<TItem> handler;

		public Command(HandlerInfo<TItem> handler, TItem arg)
		{
			this.handler = handler;
			this.arg = arg;
		}

		public void Invoke() => handler.Handler(arg);
	}
}
