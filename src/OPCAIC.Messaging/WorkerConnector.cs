using System;
using System.Threading;
using NetMQ;
using NetMQ.Sockets;
using OPCAIC.Messaging.Commands;
using OPCAIC.Messaging.Messages;
using OPCAIC.Messaging.Utils;

namespace OPCAIC.Messaging
{
	public class WorkerConnector : ConnectorBase<DealerSocket, object>
	{
		private readonly string address;
		private readonly NetMQTimer pingTimer;

		private bool connected = false;

		private int liveness = Defaults.Liveness;
		private int sleepInterval = Defaults.ReconnectIntervalInit;

		public WorkerConnector(string address, string identity)
			: base(
				identity,
				new DealerSocketFactory(identity, address, false),
				new HandlerSet<object>(obj => obj))
		{
			this.address = address;

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
				{
					sleepInterval *= 2; // exponential back off
				}
				else
				{
					throw new InvalidOperationException("The broker is unreachable");
				}

				ResetConnection();
				connected = false;
				liveness = Defaults.Liveness;
				return;
			}

			//			Console.WriteLine($"[{Identity}] - Sending ping");
			DirectSend(CreateMessage(new PingMessage()));
		}

		protected override object ReceiveMessage(NetMQMessage msg)
		{
			// treat each message as a ping message
			ResetHeartbeat();
			if (msg.First.IsEmpty)
				msg.Pop();
			return MessageHelpers.DeserializeMessage(msg);
		}

		private void ResetHeartbeat()
		{
			if (connected == false)
			{
				connected = true;
				Console.WriteLine($"[{Identity}] - connection established");
			}
			pingTimer.EnableAndReset();
			liveness = Defaults.Liveness;
			sleepInterval = Defaults.ReconnectIntervalInit;
		}

		public void SendMessage<T>(T payload)
		{
			var msg = CreateMessage(payload);
			EnqueueMessage(msg);
		}

		private NetMQMessage CreateMessage<T>(T payload)
		{
			var msg = new NetMQMessage(2);
			msg.AppendEmptyFrame();
			MessageHelpers.SerializeMessage(msg, payload);
			return msg;
		}

		public void RegisterHandler<T>(Action<T> handler)
			=> AddHandler(new HandlerInfo<object>(typeof(T), obj => handler((T) obj), true));
		public void RegisterAsyncHandler<T>(Action<T> handler)
			=> AddHandler(new HandlerInfo<object>(typeof(T), obj => handler((T) obj), false));
	}
}
