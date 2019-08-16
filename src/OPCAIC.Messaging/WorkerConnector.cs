using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NetMQ;
using NetMQ.Sockets;
using OPCAIC.Messaging.Commands;
using OPCAIC.Messaging.Config;
using OPCAIC.Messaging.Utils;

namespace OPCAIC.Messaging
{
	/// <summary>
	///     NetMQ connector for the worker node.
	/// </summary>
	public class WorkerConnector : ConnectorBase<DealerSocket, object>, IWorkerConnector
	{
		private readonly NetMQTimer incomingHeartbeatTimer;
		private readonly NetMQTimer outgoingHeartbeatTimer;

		private bool connected;
		private int liveness;
		private int sleepInterval;

		public WorkerConnector(IOptions<WorkerConnectorConfig> config,
			ILogger<WorkerConnector> logger)
			: base(
				config.Value.Identity,
				new DealerSocketFactory(config.Value.Identity, config.Value.BrokerAddress),
				new HandlerSet<object>(obj => obj),
				HeartbeatConfig.Default, // temporary, will receive correct after connection.
				logger)
		{
			// setup connection timeout, this handler will run on Socket thread
			incomingHeartbeatTimer = new NetMQTimer(HeartbeatConfig.HeartbeatInterval);
			incomingHeartbeatTimer.Elapsed += (_, a) => OnHeartBeatTimeOut();
			outgoingHeartbeatTimer = new NetMQTimer(HeartbeatConfig.HeartbeatInterval);
			outgoingHeartbeatTimer.Elapsed += (_, a) => SendHeartbeat();

			SocketPoller.Add(incomingHeartbeatTimer);
			SocketPoller.Add(outgoingHeartbeatTimer);

			liveness = HeartbeatConfig.Liveness;
			sleepInterval = HeartbeatConfig.ReconnectIntervalInit;
		}

		/// <inheritdoc cref="IWorkerConnector" />
		public event EventHandler Connected;

		/// <inheritdoc cref="IWorkerConnector" />
		public event EventHandler Disconnected;

		/// <inheritdoc cref="IWorkerConnector" />
		public void RegisterAsyncHandler<T>(Func<T, Task> handler)
		{
			AddHandler(new HandlerInfo<object>(typeof(T), obj => handler((T)obj), false));
		}

		public void RegisterAsyncHandler<T>(Action<T> handler)
		{
			RegisterAsyncHandler(WrapAction(handler));
		}

		/// <inheritdoc cref="IWorkerConnector" />
		public void RegisterHandler<T>(Func<T, Task> handler)
		{
			AddHandler(new HandlerInfo<object>(typeof(T), obj => handler((T)obj), true));
		}

		/// <inheritdoc cref="IWorkerConnector" />
		public void RegisterHandler<T>(Action<T> handler)
		{
			RegisterHandler(WrapAction(handler));
		}

		/// <inheritdoc cref="IWorkerConnector" />
		public void SendMessage(object payload)
		{
			EnqueueSocketTask(() =>
			{
				var msg = CreateMessage(payload);
				outgoingHeartbeatTimer.EnableAndReset();
				DirectSend(msg);
			});
		}

		/// <summary>
		///     Wraps the given action into a simple async action.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="handler"></param>
		/// <returns></returns>
		private static Func<T, Task> WrapAction<T>(Action<T> handler)
		{
			return p =>
			{
				handler(p);
				return Task.CompletedTask;
			};
		}

		/// <inheritdoc />
		protected override void OnHeartbeatConfigChanged(HeartbeatConfig config)
		{
			liveness = config.Liveness;
			sleepInterval = config.ReconnectIntervalInit;
			incomingHeartbeatTimer.Interval = config.HeartbeatInterval;
			outgoingHeartbeatTimer.Interval = config.HeartbeatInterval;
		}

		/// <inheritdoc />
		protected override object ReceiveMessage(NetMQMessage msg)
		{
			AssertSocketThread();
			if (connected == false)
			{
				connected = true;
				EnqueueConsumerTask(OnConnected);
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

		private void SendHeartbeat()
		{
			DirectSend(CreateMessage(null));
		}

		private void OnHeartBeatTimeOut()
		{
			AssertSocketThread();
			if (--liveness == 0)
			{
				EnqueueConsumerTask(OnDisconnected);

				if (sleepInterval <= HeartbeatConfig.ReconnectIntervalMax)
				{
					Logger.LogError(
						$"[{Identity}] - Broker unreachable, retrying in {sleepInterval} ms");
					Thread.Sleep(sleepInterval);
					sleepInterval *= 2; // exponential back off
				}
				else
				{
					throw new InvalidOperationException("The broker is unreachable");
				}

				connected = false;
				ResetConnection();
				liveness = HeartbeatConfig.Liveness;
			}
			else if (liveness < HeartbeatConfig.Liveness - 1)
			{
				Logger.LogWarning($"[{Identity}] - heartbeat timeout, liveness={liveness}");
			}
		}

		private void ResetHeartbeat()
		{
			AssertSocketThread();
			incomingHeartbeatTimer.EnableAndReset();
			liveness = HeartbeatConfig.Liveness;
			sleepInterval = HeartbeatConfig.ReconnectIntervalInit;
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
			AssertConsumerThread();
			Connected?.Invoke(this, EventArgs.Empty);
		}

		private void OnDisconnected()
		{
			AssertConsumerThread();
			Disconnected?.Invoke(this, EventArgs.Empty);
		}
	}
}