using System;
using System.Runtime.InteropServices.ComTypes;
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
		private readonly NetMQTimer incomingHeartbeatTimer;
		private readonly NetMQTimer outgoingHeartbeatTimer;

		private bool connected;
		private int liveness;
		private int sleepInterval;

		public WorkerConnector(string address, string identity, HeartbeatConfig config)
			: base(
				identity,
				new DealerSocketFactory(identity, address, false),
				new HandlerSet<object>(obj => obj),
				config)
		{
			this.address = address;

			// setup connection timeout, this handler will run on Socket thread
			incomingHeartbeatTimer = new NetMQTimer(Config.HeartbeatInterval);
			incomingHeartbeatTimer.Elapsed += (_, a) => OnPingTimeOut();
			outgoingHeartbeatTimer = new NetMQTimer(Config.HeartbeatInterval);
			outgoingHeartbeatTimer.Elapsed += (_, a) => SendHeartbeat();

			SocketPoller.Add(incomingHeartbeatTimer);
			SocketPoller.Add(outgoingHeartbeatTimer);

			liveness = Config.Liveness;
			sleepInterval = Config.ReconnectIntervalInit;
		}


		public event EventHandler Connected;

		public event EventHandler Disconnected;

		public void RegisterHandler<T>(Action<T> handler)
			=> AddHandler(new HandlerInfo<object>(typeof(T), obj => handler((T) obj), true));

		public void RegisterAsyncHandler<T>(Action<T> handler)
			=> AddHandler(new HandlerInfo<object>(typeof(T), obj => handler((T) obj), false));

		public void SendMessage<T>(T payload)
		{
			var msg = CreateMessage(payload);
			EnqueueSocketTask(() => DirectSend(msg));
		}

		protected override object ReceiveMessage(NetMQMessage msg)
		{
			AssertSocketThread();
			if (connected == false)
			{
				connected = true;
				EnqueueWorkerTask(OnConnected);
			}

			// treat each message as a heartbeat
			ResetHeartbeat();

			msg.Pop(); // empty frame
			if (msg.IsEmpty)
				return null;

			return MessageHelpers.DeserializeMessage(msg);
		}

		private void SendHeartbeat() => DirectSend(CreateMessage(null));

		private void OnPingTimeOut()
		{
			AssertSocketThread();
			if (--liveness == 0)
			{
				EnqueueWorkerTask(OnDisconnected);

				Console.WriteLine($"[{Identity}] - Broker unreachable, sleeping for {sleepInterval} ms");
				if (sleepInterval <= Config.ReconnectIntervalMax)
				{
					Thread.Sleep(sleepInterval);
					sleepInterval *= 2; // exponential back off
				}
				else
				{
					throw new InvalidOperationException("The broker is unreachable");
				}

				connected = false;
				ResetConnection();
				liveness = Config.Liveness;
			}
			else
			{
				Console.WriteLine($"[{Identity}] - ping timeout, liveness={liveness}");
			}
		}

		private void ResetHeartbeat()
		{
			AssertSocketThread();
			incomingHeartbeatTimer.EnableAndReset();
			liveness = Config.Liveness;
			sleepInterval = Config.ReconnectIntervalInit;
		}

		private NetMQMessage CreateMessage(object payload)
		{
			var msg = new NetMQMessage();
			msg.AppendEmptyFrame();
			if (payload != null)
				MessageHelpers.SerializeMessage(msg, payload);
			return msg;
		}

		private void OnConnected()
		{
			AssertWorkThread();
			Connected?.Invoke(this, EventArgs.Empty);
		}

		private void OnDisconnected()
		{
			AssertWorkThread();
			Disconnected?.Invoke(this, EventArgs.Empty);
		}
	}
}
