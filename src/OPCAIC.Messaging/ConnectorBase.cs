using System;
using System.Diagnostics;
using System.Threading.Tasks;
using NetMQ;
using OPCAIC.Messaging.Commands;
using OPCAIC.Messaging.Messages;
using OPCAIC.Messaging.Utils;

namespace OPCAIC.Messaging
{
	public class HeartbeatConfig
	{
		public static HeartbeatConfig Default
			=> new HeartbeatConfig
			{
				HeartbeatInterval = 1000,
				Liveness = 3,
				ReconnectIntervalInit = 1000,
				ReconnectIntervalMax = 32000
			};

		public int HeartbeatInterval { get; set; }
		public int Liveness { get; set; }
		public int ReconnectIntervalInit { get; set; }
		public int ReconnectIntervalMax { get; set; }
	}

	public abstract class ConnectorBase<TSocket, TItem> : IDisposable where TSocket : NetMQSocket
	{
		protected readonly HeartbeatConfig Config;

		private readonly IHandlerSet<TItem> handlerSet;
		private readonly ISocketFactory<TSocket> socketFactory;
		protected readonly NetMQPoller SocketPoller;
		protected readonly NetMQPoller WorkPoller;

		protected ConnectorBase(
			string identity,
			ISocketFactory<TSocket> socketFactory,
			IHandlerSet<TItem> handlerSet,
			HeartbeatConfig config)
		{
			Identity = identity;
			this.socketFactory = socketFactory;
			this.handlerSet = handlerSet;
			Config = config;

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
			if (!msg.IsEmpty && !msg.Last.IsEmpty)
				Console.WriteLine($"[{Identity}] - Sending {msg}");
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

		protected void AssertSocketThread()
		{
			Debug.Assert(SocketPoller.CanExecuteTaskInline, "Not called from the socket thread");
		}

		protected void AssertWorkThread()
		{
			Debug.Assert(WorkPoller.CanExecuteTaskInline, "Not called from the work thread");
		}

		private void OnPollerReceive(NetMQMessage msg)
		{
			AssertSocketThread();
			if (!msg.Last.IsEmpty)
			{
				// non-heartbeat message
				Console.WriteLine($"[{Identity}] - Received {msg}");
			}

			var item = ReceiveMessage(msg);
			if (item == null)
				return;

			var handler = handlerSet.GetHandler(item);
			if (handler == null)
			{
				Console.WriteLine($"[{Identity}] - no handler for given message type");
				return;
			}

			if (handler.IsSync)
			{
				// execute locally
				handler.Handler(item);
			}
			else
			{
				// queue execution on worker thread
				EnqueueWorkerTask(() => handler.Handler(item));
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
