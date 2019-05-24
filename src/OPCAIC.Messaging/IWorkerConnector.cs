using System;

namespace OPCAIC.Messaging
{
	public interface IWorkerConnector
	{
		/// <summary>
		///   Identity used for communication.
		/// </summary>
		string Identity { get; }

		/// <summary>
		///   Invoked when a connection is established.
		/// </summary>
		event EventHandler Connected;

		/// <summary>
		///   Invoked when disconnected.
		/// </summary>
		event EventHandler Disconnected;

		/// <summary>
		///   Registers a handler to be invoked on consumer thread when a message of given type is
		///   received.
		/// </summary>
		/// <typeparam name="T">Type of the handled message.</typeparam>
		/// <param name="handler">The handler.</param>
		void RegisterAsyncHandler<T>(Action<T> handler);

		/// <summary>
		///   Registers a handler to be invoked on socket thread when a message of given type is
		///   received.
		/// </summary>
		/// <typeparam name="T">Type of the handled message.</typeparam>
		/// <param name="handler">The handler.</param>
		void RegisterHandler<T>(Action<T> handler);

		/// <summary>
		///   Sends a message with given payload to the broker.
		/// </summary>
		/// <param name="payload"></param>
		void SendMessage(object payload);

		/// <summary>
		///   Invoked when a new message is received. This should not be used to receive messages, but
		///   only for notifications.
		/// </summary>
		event EventHandler<object> ReceivedMessage;

		/// <summary>
		///   Invoked when received a message of type for which no handler was registered.
		/// </summary>
		event EventHandler<object> UnhandledMessage;

		/// <summary>
		///   Entry point for the poller thread.
		/// </summary>
		void EnterPoller();

		/// <summary>
		///   Creates a new thread for the socket poller.
		/// </summary>
		void EnterPollerAsync();

		/// <summary>
		///   Stops the socket poller.
		/// </summary>
		void StopPoller();

		/// <summary>
		///   Entry point for the consumer thread.
		/// </summary>
		void EnterConsumer();

		/// <summary>
		///   Creates a new thread for the consumer poller.
		/// </summary>
		void EnterConsumerAsync();

		/// <summary>
		///   Stops the consumer poller.
		/// </summary>
		void StopConsumer();
	}
}
