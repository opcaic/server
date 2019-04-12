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
		private readonly NetMQQueue<Command<TItem>> inboundQueue;
		private readonly NetMQQueue<NetMQMessage> outboundQueue;
		private readonly ISocketFactory<TSocket> socketFactory;
		protected readonly NetMQPoller SocketPoller;
		protected readonly NetMQPoller WorkPoller;

		protected ConnectorBase(
			string identity,
			ISocketFactory<TSocket> factory,
			IHandlerSet<TItem> handlerSet)
		{
			socketFactory = factory;
			Identity = identity;
			this.handlerSet = handlerSet;

			// acknowledge ping messages on Socket thread
			handlerSet.AddHandler(new HandlerInfo<TItem>(typeof(PingMessage), delegate { }, true));

			inboundQueue = new NetMQQueue<Command<TItem>>();
			outboundQueue = new NetMQQueue<NetMQMessage>();

			// init Socket
			ResetConnection();

			// Socket thread callbacks
			SocketPoller = new NetMQPoller {Socket, outboundQueue};
			Socket.ReceiveReady += (_, args) => OnPollerReceive(args.Socket.ReceiveMultipartMessage());
			outboundQueue.ReceiveReady += (_, args) => Socket.SendMultipartMessage(args.Queue.Dequeue());

			// Work thread callbacks
			WorkPoller = new NetMQPoller {inboundQueue};
			inboundQueue.ReceiveReady += (_, args) => args.Queue.Dequeue().Invoke();
		}

		protected TSocket Socket { get; private set; }

		public void ResetConnection()
		{
			if (Socket != null)
			{
				Socket.Dispose();
				SocketPoller.Remove(Socket);
			}

			Socket = socketFactory.CreateSocket();
		}

		public void EnterPoller() => SocketPoller.Run();

		public void EnterPollerAsync() => SocketPoller.RunAsync();

		public void StopPoller() => SocketPoller.Stop();

		public void EnterConsumer() => WorkPoller.Run();

		public void EnterConsumerAsync() => WorkPoller.Run();

		public void StopConsumer() => WorkPoller.Stop();

		protected abstract TItem ReceiveMessage(NetMQMessage msg);

		private void OnPollerReceive(NetMQMessage msg)
		{
//			Console.WriteLine($"[{Identity}] - Received {msg}");
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
				inboundQueue.Enqueue(new Command<TItem>(handler, item));
			}
		}

		protected void AddHandler(HandlerInfo<TItem> handler) => handlerSet.AddHandler(handler);

		protected void EnqueueMessage(NetMQMessage msg) => outboundQueue.Enqueue(msg);

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
				inboundQueue.Dispose();
				outboundQueue.Dispose();
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
