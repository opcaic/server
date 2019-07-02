using System;
using OPCAIC.Messaging.Config;

namespace OPCAIC.Messaging
{
	public interface IWorkerConnector
	{
		/// <summary>
		///   Identity used for communication.
		/// </summary>
		string Identity { get; }

		/// <summary>
		///   Configuration options for heartbeat.
		/// </summary>
		HeartbeatConfig HeartbeatConfig { get; }

		/// <summary>
		///   Updates the heartbeat configuration to the given value.
		/// </summary>
		/// <param name="config">The configuration.</param>
		void SetHeartbeatConfig(HeartbeatConfig config);

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
		///   Entry point for the socket thread.
		/// </summary>
		void EnterSocket();

		/// <summary>
		///   Breaks the socket thread loop, causing the thread to return from the
		///   <see cref="EnterSocket" /> method.
		/// </summary>
		void StopSocket();

		/// <summary>
		///   Entry point for the consumer thread.
		/// </summary>
		void EnterConsumer();

		/// <summary>
		///   Breaks the consumer thread loop, causing the thread to return from the
		///   <see cref="EnterConsumer" /> method.
		/// </summary>
		void StopConsumer();
	}
}
