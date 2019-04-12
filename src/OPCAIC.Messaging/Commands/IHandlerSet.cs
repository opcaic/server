namespace OPCAIC.Messaging.Commands
{
	public interface IHandlerSet<TItem>
	{
		HandlerInfo<TItem> GetHandler(TItem workItem);

		void AddHandler(HandlerInfo<TItem> handler);
	}
}