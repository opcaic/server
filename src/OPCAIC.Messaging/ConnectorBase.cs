using System;
using NetMQ;
using OPCAIC.Messaging.Commands;
using OPCAIC.Messaging.Messages;
using OPCAIC.Messaging.Utils;

namespace OPCAIC.Messaging
{
	public abstract class ConnectorBase<TSocket, TItem> : IDisposable where TSocket : NetMQSocket
	{
		private readonly IHandlerSet<TItem> handlerSet;
		protected readonly string Identity;
		private readonly ISocketFactory<TSocket> socketFactory;
		protected readonly NetMQPoller SocketPoller;
		private readonly NetMQQueue<Action> socketQueue;
		private readonly NetMQQueue<Action> workerQueue;
		protected readonly NetMQPoller WorkPoller;

		protected ConnectorBase(
			string identity,
			ISocketFactory<TSocket> socketFactory,
			IHandlerSet<TItem> handlerSet)
		{
			Identity = identity;
			this.socketFactory = socketFactory;
			this.handlerSet = handlerSet;

			// acknowledge ping messages on Socket thread
			handlerSet.AddHandler(new HandlerInfo<TItem>(typeof(PingMessage), delegate { }, true));

			workerQueue = new NetMQQueue<Action>();
			socketQueue = new NetMQQueue<Action>();

			SocketPoller = new NetMQPoller {socketQueue};
			WorkPoller = new NetMQPoller {workerQueue};

			socketQueue.ReceiveReady += (_, args) => args.Queue.Dequeue().Invoke();
			workerQueue.ReceiveReady += (_, args) => args.Queue.Dequeue().Invoke();

			// initiate the connection
			ResetConnection();
		}

		protected TSocket Socket { get; private set; }

		public void ResetConnection()
		{
			if (Socket != null)
			{
				Socket.Close();
				Socket.Dispose();
				SocketPoller.Remove(Socket);
			}

			Socket = socketFactory.CreateSocket();
			Socket.ReceiveReady += (_, args) => OnPollerReceive(args.Socket.ReceiveMultipartMessage());
			SocketPoller.Add(Socket);
		}

		protected void DirectSend(NetMQMessage msg)
		{
			Console.WriteLine($"[{Identity}] - Sending {msg}");
			Socket.SendMultipartMessage(msg);
		}

		public void EnterPoller() => SocketPoller.Run();

		public void EnterPollerAsync() => SocketPoller.RunAsync();

		public void StopPoller() => SocketPoller.Stop();

		public void EnterConsumer() => WorkPoller.Run();

		public void EnterConsumerAsync() => WorkPoller.RunAsync();

		public void StopConsumer() => WorkPoller.Stop();

		protected abstract TItem ReceiveMessage(NetMQMessage msg);

		private void OnPollerReceive(NetMQMessage msg)
		{
			Console.WriteLine($"[{Identity}] - Received {msg}");
			var item = ReceiveMessage(msg);
			var handler = handlerSet.GetHandler(item);

			if (handler == null)
			{
				Console.WriteLine($"[{Identity}] - no handler for give message type");
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

		protected void AddHandler(HandlerInfo<TItem> handler) => handlerSet.AddHandler(handler);

		protected void EnqueueSocketTask(Action task) => socketQueue.Enqueue(task);

		protected void EnqueueWorkerTask(Action task) => workerQueue.Enqueue(task);

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
				workerQueue.Dispose();
				socketQueue.Dispose();
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
