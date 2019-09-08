using System;

namespace OPCAIC.Messaging
{
	public class TimedCallback
	{
		/// <inheritdoc />
		public TimedCallback(Action callback, TimeSpan period, bool socketThread = false)
		{
			Callback = callback;
			Period = period;
			SocketThread = socketThread;
		}

		/// <summary>
		///     Callback function to be called.
		/// </summary>
		public Action Callback { get; }

		/// <summary>
		///     True if the callback should be scheduled on the socket thread. If false, then consumer thread is used.
		/// </summary>
		public bool SocketThread { get; }

		/// <summary>
		///     How often to call the callback function.
		/// </summary>
		public TimeSpan Period { get; }
	}
}