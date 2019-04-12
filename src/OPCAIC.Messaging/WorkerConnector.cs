using System;
using System.Collections.Generic;
using System.Runtime.InteropServices.ComTypes;
using System.Text;
using System.Threading;
using NetMQ;
using NetMQ.Sockets;
using OPCAIC.Messaging.Messages;

namespace OPCAIC.Messaging
{
	public class WorkerConnector : ConnectorBase<DealerSocket>
	{
		private readonly string address;
		private readonly NetMQTimer pingTimer;
		private readonly Dictionary<Type, Action<object>> handlers;

		private int liveness = Defaults.Liveness;
		private int sleepInterval = Defaults.ReconnectIntervalInit;

		public WorkerConnector(string address, string identity)
			:base (new DealerSocketFactory(identity, address, false),  identity)
		{
			this.address = address;
			handlers = new Dictionary<Type, Action<object>>
			{
				[typeof(PingMessage)] = delegate { } // ignore ping messages
			};

			// setup connection timeout, this handler will run on Socket thread
			pingTimer = new NetMQTimer(Defaults.HeartbeatInterval);
			pingTimer.Elapsed += (_, a) => OnPingTimeOut();
			SocketPoller.Add(pingTimer);
		}

		private void OnPingTimeOut()
		{
			if (--liveness == 0)
			{
				Console.WriteLine($"[{Identity}] - Broker unreachable, sleeping for {sleepInterval} ms");
				Thread.Sleep(sleepInterval);
				if (sleepInterval < Defaults.ReconnectIntervalMax)
					sleepInterval *= 2; // exponential back off
				else
				{
					throw new InvalidOperationException("The broker is unreachable");
				}
				ResetConnection();
				liveness = Defaults.Liveness;
			}
			Console.WriteLine($"[{Identity}] - Sending ping");
			Socket.SendMultipartMessage(CreateMessage(new PingMessage()));
		}

		void ResetConnection()
		{
			Socket.Disconnect(address);
			Socket.Connect(address);
		}

		protected override void OnPollerReceive(NetMQMessage msg)
		{
			// treat each message as a ping message
			pingTimer.EnableAndReset();

			// reset reconnection props
			liveness = Defaults.Liveness;
			sleepInterval = Defaults.ReconnectIntervalInit;
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