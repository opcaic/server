namespace OPCAIC.Messaging.Commands
{
	/// <summary>
	///     Interface for the set of message handlers inside a connector.
	/// </summary>
	/// <typeparam name="TItem"></typeparam>
	public interface IHandlerSet<TItem>
	{
		/// <summary>
		///     Gets handler based on the work item.
		/// </summary>
		/// <param name="workItem">Work item to be handled.</param>
		/// <returns>The responsible handler, or null if no such handler exists.</returns>
		HandlerInfo<TItem> GetHandler(TItem workItem);

		/// <summary>
		///     Adds a new handler
		/// </summary>
		/// <param name="handler">The handler.</param>
		void AddHandler(HandlerInfo<TItem> handler);
	}
}