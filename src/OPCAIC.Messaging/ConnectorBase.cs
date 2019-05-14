using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using NetMQ;
using OPCAIC.Messaging.Commands;
using OPCAIC.Messaging.Config;
using OPCAIC.Messaging.Utils;

namespace OPCAIC.Messaging
{
	public abstract class ConnectorBase<TSocket, TItem> : IDisposable where TSocket : NetMQSocket
	{
		protected readonly HeartbeatConfig Config;

		private readonly IHandlerSet<TItem> handlerSet;
		protected readonly ILogger Logger;
		private readonly ISocketFactory<TSocket> socketFactory;
		protected readonly NetMQPoller SocketPoller;
		protected readonly NetMQPoller WorkPoller;

		public event EventHandler<TItem> MessageReceived;
		public event EventHandler<TItem> UnhandeledMessage;

		protected ConnectorBase(string identity,
			ISocketFactory<TSocket> socketFactory,
			IHandlerSet<TItem> handlerSet,
			HeartbeatConfig config,
			ILogger logger)
		{
			Identity = identity;
			this.socketFactory = socketFactory;
			this.handlerSet = handlerSet;
			Config = config;
			Logger = logger;

			SocketPoller = new NetMQPoller();
			WorkPoller = new NetMQPoller();

			// initiate the connection
			ResetConnection();
		}

		public string Identity { get; }

		protected TSocket Socket { get; private set; }

		protected void ResetConnection()
		{
			if (Socket != null)
			{
				AssertSocketThread(); // allow first call in constructor from foreign thread
				Socket.Close();
				Socket.Dispose();
				SocketPoller.Remove(Socket);
			}

			Socket = socketFactory.CreateSocket();
			Socket.ReceiveReady += (_, args) => OnPollerReceive(args.Socket.ReceiveMultipartMessage());
			SocketPoller.Add(Socket);
		}

		public void EnterPoller() => SocketPoller.Run();

		public void EnterPollerAsync() => SocketPoller.RunAsync();

		public void StopPoller() => SocketPoller.Stop();

		public void EnterConsumer() => WorkPoller.Run();

		public void EnterConsumerAsync() => WorkPoller.RunAsync();

		public void StopConsumer() => WorkPoller.Stop();

		protected void DirectSend(NetMQMessage msg)
		{
			AssertSocketThread();
			// avoid calling msg.ToString if debug is off
			if (!msg.IsEmpty && !msg.Last.IsEmpty && Logger.IsEnabled(LogLevel.Debug))
			{
				Logger.LogDebug($"[{Identity}] - Sending {msg}");
			}
			Socket.SendMultipartMessage(msg);
		}

		protected abstract TItem ReceiveMessage(NetMQMessage msg);

		protected void AddHandler(HandlerInfo<TItem> handler) => handlerSet.AddHandler(handler);

		protected Task EnqueueSocketTask(Action task)
		{
			var t = new Task(task);
			t.Start(SocketPoller);
			return t;
		}

		protected Task EnqueueWorkerTask(Action task)
		{
			var t = new Task(task);
			t.Start(WorkPoller);
			return t;
		}

		protected void EnqueueWorkerTask(Task task) => task.Start(WorkPoller);

		protected void AssertSocketThread()
			=> Debug.Assert(SocketPoller.CanExecuteTaskInline, "Not called from the socket thread");

		protected void AssertWorkThread()
			=> Debug.Assert(WorkPoller.CanExecuteTaskInline, "Not called from the work thread");

		private void OnPollerReceive(NetMQMessage msg)
		{
			AssertSocketThread();
			// avoid calling msg.ToString if debug is off
			if (!msg.Last.IsEmpty && Logger.IsEnabled(LogLevel.Debug))
			{
				Logger.LogDebug($"[{Identity}] - Received {msg}");
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
				EnqueueWorkerTask(() => OnUnhandledMessage(item));
			}
			else if (handler.IsSync)
			{
				// execute locally
				handler.Handler(item);
			}
			else
			{
				// queue execution on worker thread
				EnqueueWorkerTask(() => handler.Handler(item));
			}

			EnqueueWorkerTask(() => OnMessageReceived(item));
		}

		private void OnUnhandledMessage(TItem item)
		{
			AssertWorkThread();
			UnhandeledMessage?.Invoke(this, item);
		}

		private void OnMessageReceived(TItem message)
		{
			AssertWorkThread();
			MessageReceived?.Invoke(this, message);
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
				WorkPoller.Dispose();
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
