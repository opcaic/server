using System;
using System.Threading.Tasks;
using OPCAIC.Messaging.Config;

namespace OPCAIC.Messaging
{
	public interface IBrokerConnector
	{
		/// <summary>
		///     Identity used for communication.
		/// </summary>
		string Identity { get; }

		/// <summary>
		///     Configuration options for heartbeat.
		/// </summary>
		HeartbeatConfig HeartbeatConfig { get; }

		/// <summary>
		///     Updates the heartbeat configuration to the given value.
		/// </summary>
		/// <param name="config">The configuration.</param>
		void SetHeartbeatConfig(HeartbeatConfig config);


		/// <summary>
		///     Invoked when a new worker has connected.
		/// </summary>
		event EventHandler<WorkerConnectionEventArgs> WorkerDisconnected;

		/// <summary>
		///     Invoked when a worker has disconnected.
		/// </summary>
		event EventHandler<WorkerConnectionEventArgs> WorkerConnected;

		/// <summary>
		///     Registers a handler to be invoked on consumer thread when a message of given type is
		///     received.
		/// </summary>
		/// <typeparam name="T">Type of the handled message.</typeparam>
		/// <param name="handler">The handler.</param>
		void RegisterAsyncHandler<T>(Action<string, T> handler);

		/// <summary>
		///     Registers a handler to be invoked on consumer thread when a message of given type is
		///     received.
		/// </summary>
		/// <typeparam name="T">Type of the handled message.</typeparam>
		/// <param name="handler">The handler.</param>
		void RegisterAsyncHandler<T>(Func<string, T, Task> handler);

		/// <summary>
		///     Registers a handler to be invoked on socket thread when a message of given type is
		///     received.
		/// </summary>
		/// <typeparam name="T">Type of the handled message.</typeparam>
		/// <param name="handler">The handler.</param>
		void RegisterHandler<T>(Action<string, T> handler);

		/// <summary>
		///     Registers a handler to be invoked on socket thread when a message of given type is
		///     received.
		/// </summary>
		/// <typeparam name="T">Type of the handled message.</typeparam>
		/// <param name="handler">The handler.</param>
		void RegisterHandler<T>(Func<string, T, Task> handler);

		/// <summary>
		///     Sends a message to the specified worker with given payload.
		/// </summary>
		/// <param name="recipient">The recipients identity.</param>
		/// <param name="payload">Payload to send.</param>
		void SendMessage(string recipient, object payload);

		/// <summary>
		///     Enqueues a task to be executed on the consumer thread.
		/// </summary>
		/// <param name="task"></param>
		void EnqueueTask(Task task);

		/// <summary>
		///     Invoked when a new message is received. This should not be used to receive messages, but
		///     only for notifications.
		/// </summary>
		event EventHandler<ReceivedMessage> ReceivedMessage;

		/// <summary>
		///     Invoked when received a message of type for which no handler was registered.
		/// </summary>
		event EventHandler<ReceivedMessage> UnhandledMessage;

		/// <summary>
		///     Entry point for the poller thread.
		/// </summary>
		void EnterSocket();

		/// <summary>
		///     Creates a new thread for the socket poller.
		/// </summary>
		void EnterSocketAsync();

		/// <summary>
		///     Breaks the socket thread loop, causing the thread to return from the
		///     <see cref="EnterSocket" /> method.
		/// </summary>
		void StopSocket();

		/// <summary>
		///     Entry point for the consumer thread.
		/// </summary>
		void EnterConsumer();

		/// <summary>
		///     Creates a new thread for the consumer poller.
		/// </summary>
		void EnterConsumerAsync();

		/// <summary>
		///     Breaks the consumer thread loop, causing the thread to return from the
		///     <see cref="EnterConsumer" /> method.
		/// </summary>
		void StopConsumer();
	}
}