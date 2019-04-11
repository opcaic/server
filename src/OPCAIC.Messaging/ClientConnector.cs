using System;
using System.Collections.Generic;
using System.Runtime.InteropServices.ComTypes;
using System.Text;
using NetMQ;
using NetMQ.Sockets;

namespace OPCAIC.Messaging
{
	public class ClientConnector : ConnectorBase<DealerSocket>
	{
		private readonly string address;
		private readonly NetMQTimer pingTimer;
		private readonly Dictionary<Type, Action<object>> handlers;

		public ClientConnector(string address, string identity)
			:base (new DealerSocket(address), identity)
		{
			this.address = address;
			handlers = new Dictionary<Type, Action<object>>
			{
				[typeof(PingMessage)] = delegate { } // ignore ping messages
			};

			// setup connection timeout, this handler will run on Socket thread
			pingTimer = new NetMQTimer(Defaults.PingTimeout);
			pingTimer.Elapsed += (_, a) => OnPingTimeOut();
			SocketPoller.Add(pingTimer);
		}

		private void OnPingTimeOut()
		{
			Console.WriteLine($"[{Identity}] - Ping timeout");
			Socket.SendMultipartMessage(CreateMessage(new PingMessage()));
		}

		protected override void OnPollerReceive(NetMQMessage msg)
		{
			// treat each message as a ping message
			pingTimer.EnableAndReset();
			base.OnPollerReceive(msg);
		}

		protected override void OnMessage(NetMQMessage msg)
		{
			object payload = DeserializeMessage(msg);
			if (!handlers.TryGetValue(payload.GetType(), out var handler))
			{
				throw new InvalidOperationException("Unknown payload type");
			}

			handler(payload);
		}

		public void SendMessage<T>(T payload)
		{
			var msg = CreateMessage(payload);
			EnqueueMessage(msg);
		}

		private NetMQMessage CreateMessage<T>(T payload)
		{
			NetMQMessage msg = new NetMQMessage(2);
//			msg.AppendEmptyFrame();
			SerializeMessage(msg, payload);
			return msg;
		}

		public void RegisterHandler<T>(Action<T> handler)
		{
			handlers.Add(typeof(T), obj=>handler((T) obj));
		}
	}
}