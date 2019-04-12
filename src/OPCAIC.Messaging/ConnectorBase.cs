using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.Serialization.Json;
using System.Text;
using NetMQ;
using Newtonsoft.Json;

namespace OPCAIC.Messaging
{
	public abstract class ConnectorBase<TSocket>: IDisposable where TSocket : NetMQSocket
	{
		private readonly NetMQQueue<NetMQMessage> inboundQueue;
		private readonly NetMQQueue<NetMQMessage> outboundQueue;

		private readonly ISocketFactory<TSocket> socketFactory;
		protected readonly string Identity;
		protected TSocket Socket { get; private set; }
		protected readonly NetMQPoller SocketPoller;
		protected readonly NetMQPoller WorkPoller;

		protected ConnectorBase(ISocketFactory<TSocket> factory, string identity)
		{
			socketFactory = factory;
			Identity = identity;
			ResetConnection();

			inboundQueue = new NetMQQueue<NetMQMessage>();
			outboundQueue = new NetMQQueue<NetMQMessage>();

			// Socket thread callbacks
			SocketPoller = new NetMQPoller {Socket, outboundQueue};
			Socket.ReceiveReady += (_, args) => OnPollerReceive(args.Socket.ReceiveMultipartMessage());
			outboundQueue.ReceiveReady += (_, args) => Socket.SendMultipartMessage(args.Queue.Dequeue());

			// Work thread callbacks
			WorkPoller = new NetMQPoller {inboundQueue};
			inboundQueue.ReceiveReady += (_, args) => OnMessage(args.Queue.Dequeue());
		}

		public void ResetConnection()
		{
			Socket?.Dispose();
			Socket = socketFactory.CreateSocket();
		}
		public void EnterPoller() => SocketPoller.Run();

		public void EnterPollerAsync() => SocketPoller.RunAsync();

		public void StopPoller() => SocketPoller.Stop();

		public void EnterConsumer() => WorkPoller.Run();

		public void EnterConsumerAsync() => WorkPoller.Run();

		public void StopConsumer() => WorkPoller.Stop();

		protected abstract void OnMessage(NetMQMessage msg);

		protected virtual void OnPollerReceive(NetMQMessage msg)
		{
//			Console.WriteLine($"[{Identity}] - Received {msg}");
			inboundQueue.Enqueue(msg);
		}

		protected void SerializeMessage(NetMQMessage msg, object payload)
		{
//			var bf = new BinaryFormatter();
//			var ms = new MemoryStream();
//			bf.Serialize(ms, payload);
//			msg.Append(new NetMQFrame(ms.GetBuffer(), (int) ms.Length));
			msg.Append(JsonConvert.SerializeObject(payload, new JsonSerializerSettings()
			{
				TypeNameHandling = TypeNameHandling.All
			}));
		}

		protected object DeserializeMessage(NetMQMessage msg)
		{
//			var frame = msg.Pop();
//			var ms = new MemoryStream(frame.Buffer, 0, frame.BufferSize);
//			var bf = new BinaryFormatter();
//			return bf.Deserialize(ms);
			return JsonConvert.DeserializeObject(msg.Pop().ConvertToString(), new JsonSerializerSettings()
			{
				TypeNameHandling = TypeNameHandling.All
			});
		}

		protected void EnqueueMessage(NetMQMessage msg) => outboundQueue.Enqueue(msg);

		#region IDisposable Support
		private bool disposedValue = false; // To detect redundant calls

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
