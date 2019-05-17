using System;
using System.Collections.Generic;

namespace OPCAIC.Messaging.Commands
{
	/// <summary>
	///   Concrete generic implementation of handler set.
	/// </summary>
	/// <typeparam name="TItem"></typeparam>
	public class HandlerSet<TItem> : IHandlerSet<TItem>
	{
		private readonly Dictionary<Type, HandlerInfo<TItem>> handlers;
		private readonly Func<TItem, object> payloadSelector;

		public HandlerSet(Func<TItem, object> payloadSelector)
		{
			this.payloadSelector = payloadSelector;
			handlers = new Dictionary<Type, HandlerInfo<TItem>>();
		}

		/// <inheritdoc />
		public HandlerInfo<TItem> GetHandler(TItem workItem)
		{
			handlers.TryGetValue(payloadSelector(workItem).GetType(), out var handler);
			return handler;
		}

		/// <inheritdoc />
		public void AddHandler(HandlerInfo<TItem> info) => handlers.Add(info.Discriminator, info);
	}
}
