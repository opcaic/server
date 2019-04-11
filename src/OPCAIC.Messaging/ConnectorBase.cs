using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.Serialization.Json;
using System.Text;
using NetMQ;
using Newtonsoft.Json;

namespace OPCAIC.Messaging
{
	public abstract class ConnectorBase<TSocket> where TSocket : NetMQSocket
	{
		private readonly NetMQQueue<NetMQMessage> inboundQueue;
		private readonly NetMQQueue<NetMQMessage> outboundQueue;

		protected readonly string Identity;
		protected readonly TSocket Socket;
		protected readonly NetMQPoller SocketPoller;
		protected readonly NetMQPoller WorkPoller;

		public ConnectorBase(TSocket socket, string identity)
		{
			this.Identity = identity;
			Socket = socket;
			Socket.Options.Identity = Encoding.Unicode.GetBytes(identity);

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
		public void EnterPoller() => SocketPoller.Run();
		public void EnterConsumer() => WorkPoller.Run();

		protected abstract void OnMessage(NetMQMessage msg);

		protected virtual void OnPollerReceive(NetMQMessage msg)
		{
			Console.WriteLine($"[{Identity}] - Received {msg}");
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
	}
}
