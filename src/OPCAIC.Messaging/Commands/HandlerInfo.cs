using System;
using System.Threading.Tasks;

namespace OPCAIC.Messaging.Commands
{
	/// <summary>
	///     Information about a connectors message handler.
	/// </summary>
	/// <typeparam name="TPayload"></typeparam>
	public class HandlerInfo<TPayload>
	{
		public HandlerInfo(Type discriminator, Func<TPayload, Task> handler, bool isSync)
		{
			Handler = handler;
			IsSync = isSync;
			Discriminator = discriminator;
		}

		/// <summary>
		///     The actual handler ofr the message.
		/// </summary>
		public Func<TPayload, Task> Handler { get; }

		/// <summary>
		///     Type of the handled messages.
		/// </summary>
		public Type Discriminator { get; }

		/// <summary>
		///     True if the handler should be invoked on socket thread. False if on consumer thread.
		/// </summary>
		public bool IsSync { get; }
	}
}