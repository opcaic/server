﻿using System;
using System.Threading.Tasks;
using NetMQ;

namespace OPCAIC.Messaging
{
	public interface IBrokerConnector
	{
		/// <summary>
		///   Identity used for communication.
		/// </summary>
		string Identity { get; }

		/// <summary>
		///   Invoked when a new worker has connected.
		/// </summary>
		event EventHandler<WorkerConnectionEventArgs> WorkerDisconnected;

		/// <summary>
		///   Invoked when a worker has disconnected.
		/// </summary>
		event EventHandler<WorkerConnectionEventArgs> WorkerConnected;

		/// <summary>
		///   Registers a handler to be invoked on consumer thread when a message of given type is
		///   received.
		/// </summary>
		/// <typeparam name="T">Type of the handled message.</typeparam>
		/// <param name="handler">The handler.</param>
		void RegisterAsyncHandler<T>(Action<string, T> handler);

		/// <summary>
		///   Registers a handler to be invoked on socket thread when a message of given type is
		///   received.
		/// </summary>
		/// <typeparam name="T">Type of the handled message.</typeparam>
		/// <param name="handler">The handler.</param>
		void RegisterHandler<T>(Action<string, T> handler);

		/// <summary>
		///   Sends a message to the specified worker with given payload.
		/// </summary>
		/// <param name="recipient">The recipients identity.</param>
		/// <param name="payload">Payload to send.</param>
		void SendMessage(string recipient, object payload);

		/// <summary>
		///   Enqueues a task to be executed on the consumer thread.
		/// </summary>
		/// <param name="task"></param>
		void EnqueueTask(Task task);

		/// <summary>
		///   Invoked when a new message is received. This should not be used to receive messages, but
		///   only for notifications.
		/// </summary>
		event EventHandler<ReceivedMessage> ReceivedMessage;

		/// <summary>
		///   Invoked when received a message of type for which no handler was registered.
		/// </summary>
		event EventHandler<ReceivedMessage> UnhandledMessage;

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
