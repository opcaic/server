using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using NetMQ;
using OPCAIC.Messaging.Commands;
using OPCAIC.Messaging.Config;
using OPCAIC.Messaging.Utils;

namespace OPCAIC.Messaging
{
	/// <summary>
	///     Base class for NetMQ based connectors.
	/// </summary>
	/// <typeparam name="TSocket">Type of the underlying NetMQ socket.</typeparam>
	/// <typeparam name="TItem">Base type of the sent messages.</typeparam>
	public abstract class ConnectorBase<TSocket, TItem> : IDisposable where TSocket : NetMQSocket
	{
		private readonly Dictionary<TimedCallback, NetMQTimer> customTimers;

		/// <summary>
		///     Poller for workload tasks.
		/// </summary>
		protected readonly NetMQPoller ConsumerPoller;

		private readonly IHandlerSet<TItem> handlerSet;

		/// <summary>
		///     Logger of this class.
		/// </summary>
		protected readonly ILogger Logger;

		private readonly ISocketFactory<TSocket> socketFactory;

		/// <summary>
		///     Poller for tasks concerning the actual NetMQSocket.
		/// </summary>
		protected readonly NetMQPoller SocketPoller;

		protected ConnectorBase(string identity,
			ISocketFactory<TSocket> socketFactory,
			IHandlerSet<TItem> handlerSet,
			HeartbeatConfig config,
			ILogger logger)
		{
			this.socketFactory = socketFactory;
			this.handlerSet = handlerSet;
			HeartbeatConfig = config;
			Logger = logger;

			SocketPoller = new NetMQPoller();
			ConsumerPoller = new NetMQPoller();
			customTimers = new Dictionary<TimedCallback, NetMQTimer>();

			// initiate the connection
			ResetConnection();
		}

		/// <summary>
		///     Heartbeat configuration.
		/// </summary>
		public HeartbeatConfig HeartbeatConfig { get; private set; }

		/// <summary>
		///     Identity used for communication.
		/// </summary>
		public string Identity => socketFactory.Identity;


		/// <summary>
		///     The underlying NetMQ Socket.
		/// </summary>
		protected TSocket Socket { get; private set; }

		/// <summary>
		///     Sets heartbeat configuration for this connector.
		/// </summary>
		/// <param name="config"></param>
		public void SetHeartbeatConfig(HeartbeatConfig config)
		{
			AssertSocketThread();
			HeartbeatConfig = config;
			OnHeartbeatConfigChanged(config);
		}

		/// <summary>
		///     Updates the necessary state when heartbeat configuration changes.
		/// </summary>
		/// <param name="config"></param>
		protected abstract void OnHeartbeatConfigChanged(HeartbeatConfig config);

		/// <summary>
		///     Invoked when a new message is received. This should not be used to receive messages, but
		///     only for notifications.
		/// </summary>
		public event EventHandler<TItem> ReceivedMessage;

		/// <summary>
		///     Invoked when received a message of type for which no handler was registered.
		/// </summary>
		public event EventHandler<TItem> UnhandledMessage;

		/// <summary>
		///     Resets connection by creating a new socket.
		/// </summary>
		protected void ResetConnection()
		{
			if (Socket != null)
			{
				// keep the assertion inside if block to allow the first call (in class constructor)
				// to be called from foreign thread
				AssertSocketThread(); 

				SocketPoller.Remove(Socket);
				Socket.Dispose();
			}

			Socket = socketFactory.CreateSocket();
			Socket.ReceiveReady +=
				async (_, args) => await OnSocketReceive(args.Socket.ReceiveMultipartMessage());
			SocketPoller.Add(Socket);
		}

		/// <summary>
		///     Entry point for the poller thread.
		/// </summary>
		public void EnterSocket()
		{
			SocketPoller.Run();
		}

		/// <summary>
		///     Creates a new thread for the socket poller.
		/// </summary>
		public void EnterSocketAsync()
		{
			SocketPoller.RunAsync();
		}

		/// <summary>
		///     Breaks the socket thread loop, causing the thread to return from the
		///     <see cref="EnterSocket" /> method.
		/// </summary>
		public void StopSocket()
		{
			SocketPoller.StopAsync();
		}

		/// <summary>
		///     Entry point for the consumer thread.
		/// </summary>
		public void EnterConsumer()
		{
			ConsumerPoller.Run();
		}

		/// <summary>
		///     Creates a new thread for the consumer poller.
		/// </summary>
		public void EnterConsumerAsync()
		{
			ConsumerPoller.RunAsync();
		}

		/// <summary>
		///     Breaks the consumer thread loop, causing the thread to return from the
		///     <see cref="EnterConsumer" /> method.
		/// </summary>
		public void StopConsumer()
		{
			ConsumerPoller.StopAsync();
		}

		/// <summary>
		///     Performs the actual send of the given NetMQMessage.
		/// </summary>
		/// <param name="msg">Message to send.</param>
		protected void DirectSend(NetMQMessage msg)
		{
			AssertSocketThread();
			// avoid calling msg.ToString if debug is off
			if (!msg.IsEmpty && !msg.Last.IsEmpty && Logger.IsEnabled(LogLevel.Trace))
			{
				Logger.LogTrace($"[{Identity}] - Sending {msg}");
			}

			Socket.SendMultipartMessage(msg);
		}

		/// <summary>
		///     Callback for received NetMQMessages for creating more processable messages.
		/// </summary>
		/// <param name="msg">The received message.</param>
		/// <returns></returns>
		protected abstract TItem ReceiveMessage(NetMQMessage msg);

		/// <summary>
		///     Adds a new message handler.
		/// </summary>
		/// <param name="handler">The handler.</param>
		protected void AddHandler(HandlerInfo<TItem> handler)
		{
			handlerSet.AddHandler(handler);
		}

		/// <summary>
		///     Enqueues a new task to be done by the socket thread.
		/// </summary>
		/// <param name="task">The action to be performed in the task.</param>
		protected void EnqueueSocketTask(Action task)
		{
			new Task(task).Start(SocketPoller);
		}

		/// <summary>
		///     Enqueues a new task to be done by the consumer thread.
		/// </summary>
		/// <param name="task">The action to be performed in the task.</param>
		protected void EnqueueConsumerTask(Func<Task> task)
		{
			Task.Factory.StartNew(task, CancellationToken.None, TaskCreationOptions.None,
				ConsumerPoller);
		}

		/// <summary>
		///     Enqueues a new task to be done by the consumer thread.
		/// </summary>
		/// <param name="task">The action to be performed in the task.</param>
		protected void EnqueueConsumerTask(Action task)
		{
			EnqueueConsumerTask(new Task(task));
		}

		/// <summary>
		///     Enqueues a new task to be done by the consumer thread.
		/// </summary>
		protected void EnqueueConsumerTask(Task task)
		{
			task.Start(ConsumerPoller);
		}

		/// <summary>
		///     Debug assert which checks if the caller is running in socket thread.
		/// </summary>
		protected void AssertSocketThread()
		{
			Debug.Assert(SocketPoller.CanExecuteTaskInline, "Not called from the socket thread");
		}

		/// <summary>
		///     Debug assert which checks if the caller is running in the consumer thread.
		/// </summary>
		protected void AssertConsumerThread()
		{
			Debug.Assert(ConsumerPoller.CanExecuteTaskInline, "Not called from the work thread");
		}

		private async Task OnSocketReceive(NetMQMessage msg)
		{
			AssertSocketThread();
			// avoid calling msg.ToString if debug is off
			if (!msg.Last.IsEmpty && Logger.IsEnabled(LogLevel.Trace))
			{
				Logger.LogTrace($"[{Identity}] - Received {msg}");
			}

			var item = ReceiveMessage(msg);
			if (item == null)
			{
				return;
			}

			var handler = handlerSet.GetHandler(item);
			if (handler == null)
			{
				Logger.LogWarning($"[{Identity}] - no handler for given message type");
				EnqueueConsumerTask(() => OnUnhandledMessage(item));
			}
			else if (handler.IsSync)
			{
				// execute locally
				await handler.Handler(item);
			}
			else
			{
				// queue execution on worker thread
				EnqueueConsumerTask(() => handler.Handler(item));
			}

			EnqueueConsumerTask(() => OnMessageReceived(item));
		}

		private void OnUnhandledMessage(TItem item)
		{
			AssertConsumerThread();
			UnhandledMessage?.Invoke(this, item);
		}

		private void OnMessageReceived(TItem message)
		{
			AssertConsumerThread();
			ReceivedMessage?.Invoke(this, message);
		}

		public void RegisterTimer(TimedCallback callback)
		{
			var timer = new NetMQTimer(callback.Period);
			timer.Elapsed += (_, e) =>
			{
				callback.Callback();
				e.Timer.EnableAndReset();
			};
			customTimers.Add(callback, timer);

			if (callback.SocketThread)
			{
				SocketPoller.Add(timer);
			}
			else
			{
				ConsumerPoller.Add(timer);
			}
		}

		public void UnregisterTimer(TimedCallback callback)
		{
			var timer = customTimers[callback];
			customTimers.Remove(callback);

			if (callback.SocketThread)
			{
				SocketPoller.Remove(timer);
			}
			else
			{
				ConsumerPoller.Remove(timer);
			}
		}

		#region IDisposable Support

		private bool disposedValue; // To detect redundant calls

		protected virtual void Dispose(bool disposing)
		{
			if (disposing)
			{
				Socket?.Close();
				Socket?.Dispose();
				SocketPoller.Dispose();
				ConsumerPoller.Dispose();
			}
		}

		public void Dispose()
		{
			if (!disposedValue)
			{
				Dispose(true);
				disposedValue = true;
			}
		}

		#endregion
	}
}