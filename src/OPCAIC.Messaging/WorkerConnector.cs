using System;
using System.Threading;
using Microsoft.Extensions.Logging;
using NetMQ;
using NetMQ.Sockets;
using OPCAIC.Messaging.Commands;
using OPCAIC.Messaging.Utils;

namespace OPCAIC.Messaging
{
	public class WorkerConnector : ConnectorBase<DealerSocket, object>
	{
		private readonly NetMQTimer incomingHeartbeatTimer;
		private readonly NetMQTimer outgoingHeartbeatTimer;

		private bool connected;
		private int liveness;
		private int sleepInterval;

		public WorkerConnector(WorkerConnectorConfig config, ILogger<WorkerConnector> logger)
			: base(
				config.Identity,
				new DealerSocketFactory(config.Identity, config.BrokerAddress),
				new HandlerSet<object>(obj => obj),
				config.HeartbeatConfig,
				logger)
		{
			// setup connection timeout, this handler will run on Socket thread
			incomingHeartbeatTimer = new NetMQTimer(Config.HeartbeatInterval);
			incomingHeartbeatTimer.Elapsed += (_, a) => OnHeartBeatTimeOut();
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

		public void SendMessage(object payload)
			=> EnqueueSocketTask(() =>
			{
				var msg = CreateMessage(payload);
				outgoingHeartbeatTimer.EnableAndReset();
				DirectSend(msg);
			});

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
			{
				return null;
			}

			return MessageHelpers.DeserializeMessage(msg);
		}

		private void SendHeartbeat() => DirectSend(CreateMessage(null));

		private void OnHeartBeatTimeOut()
		{
			AssertSocketThread();
			if (--liveness == 0)
			{
				EnqueueWorkerTask(OnDisconnected);

				if (sleepInterval <= Config.ReconnectIntervalMax)
				{
					Logger.LogError($"[{Identity}] - Broker unreachable, retrying in {sleepInterval} ms");
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
			else if (liveness < Config.Liveness - 1)
			{
				Logger.LogWarning($"[{Identity}] - heartbeat timeout, liveness={liveness}");
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
			{
				MessageHelpers.SerializeMessage(msg, payload);
			}

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
