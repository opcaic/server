using System;

namespace OPCAIC.Messaging.Commands
{
	public class HandlerInfo<TPayload>
	{
		public HandlerInfo(Type discriminator, Action<TPayload> handler, bool isSync)
		{
			Handler = handler;
			IsSync = isSync;
			Discriminator = discriminator;
		}

		public Action<TPayload> Handler { get; }
		public Type Discriminator { get; }
		public bool IsSync { get; }
	}
}