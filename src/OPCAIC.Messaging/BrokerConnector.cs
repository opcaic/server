using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
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
	///     NetMQ connector for the broker node.
	/// </summary>
	public class BrokerConnector : ConnectorBase<RouterSocket, ReceivedMessage>, IBrokerConnector
	{
		private readonly Dictionary<string, WorkerConnection> workers;

		public BrokerConnector(IOptions<BrokerConnectorConfig> config,
			ILogger<BrokerConnector> logger)
			: base(
				config.Value.Identity,
				new RouterSocketFactory(config.Value.Identity, config.Value.ListeningAddress),
				new HandlerSet<ReceivedMessage>(msg => msg.Payload),
				config.Value.HeartbeatConfig,
				logger)
		{
			workers = new Dictionary<string, WorkerConnection>();
		}

		/// <inheritdoc cref="IBrokerConnector" />
		public event EventHandler<WorkerConnectionEventArgs> WorkerDisconnected;

		/// <inheritdoc cref="IBrokerConnector" />
		public event EventHandler<WorkerConnectionEventArgs> WorkerConnected;

		/// <inheritdoc cref="IBrokerConnector" />
		public void RegisterAsyncHandler<T>(Action<string, T> handler)
		{
			RegisterAsyncHandler(WrapAction(handler));
		}

		/// <inheritdoc cref="IBrokerConnector" />
		public void RegisterHandler<T>(Action<string, T> handler)
		{
			RegisterHandler(WrapAction(handler));
		}

		/// <inheritdoc cref="IBrokerConnector" />
		public void SendMessage(string recipient, object payload)
		{
			EnqueueSocketTask(() =>
			{
				// treat each message as a heartbeat
				if (workers.TryGetValue(Identity, out var entry))
					// the worker might have been removed
				{
					entry.OutgoingHeartbeatTimer.EnableAndReset();
				}

				DirectSend(CreateMessage(recipient, payload));
			});
		}

		/// <inheritdoc cref="IBrokerConnector" />
		public void EnqueueTask(Task task)
		{
			EnqueueConsumerTask(task);
		}

		/// <inheritdoc cref="IBrokerConnector" />
		public void RegisterAsyncHandler<T>(Func<string, T, Task> handler)
		{
			AddHandler(new HandlerInfo<ReceivedMessage>(typeof(T),
				msg => handler(msg.Sender, (T)msg.Payload), false));
		}

		/// <inheritdoc cref="IBrokerConnector" />
		public void RegisterHandler<T>(Func<string, T, Task> handler)
		{
			AddHandler(new HandlerInfo<ReceivedMessage>(typeof(T),
				msg => handler(msg.Sender, (T)msg.Payload), true));
		}

		/// <summary>
		///     Wraps the given action into a simple async action.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="handler"></param>
		/// <returns></returns>
		private static Func<string, T, Task> WrapAction<T>(Action<string, T> handler)
		{
			return (i, p) =>
			{
				handler(i, p);
				return Task.CompletedTask;
			};
		}

		/// <inheritdoc />
		protected override void OnHeartbeatConfigChanged(HeartbeatConfig config)
		{
			foreach (var worker in workers.Values)
			{
				worker.Liveness = config.Liveness;
				worker.IncomingHeartbeatTimer.Interval = config.HeartbeatInterval;
				worker.OutgoingHeartbeatTimer.Interval = config.HeartbeatInterval;
			}
		}

		/// <inheritdoc />
		protected override ReceivedMessage ReceiveMessage(NetMQMessage msg)
		{
			AssertSocketThread();
			var sender = msg.Pop().ConvertToIdentity();

			if (workers.TryGetValue(sender, out var entry))
			{
				// treat each message as a ping message
				ResetHeartbeat(entry);
			}
			else
			{
				entry = NewWorker(sender);
				EnqueueConsumerTask(() => OnWorkerConnected(entry));
			}

			msg.Pop(); // empty frame
			if (msg.IsEmpty)
			{
				return null; // heartbeat messsage
			}

			return new ReceivedMessage(sender, MessageHelpers.DeserializeMessage(msg));
		}

		private NetMQMessage CreateMessage(string recipient, object payload)
		{
			var msg = new NetMQMessage(2);
			msg.AppendIdentity(recipient);
			msg.AppendEmptyFrame();
			if (payload != null)
			{
				MessageHelpers.SerializeMessage(msg, payload);
			}

			return msg;
		}

		private void ResetHeartbeat(WorkerConnection connection)
		{
			AssertSocketThread();
			connection.IncomingHeartbeatTimer.EnableAndReset();
			connection.Liveness = HeartbeatConfig.Liveness;
		}

		private void OnWorkerDisconnected(WorkerConnection worker)
		{
			AssertConsumerThread();
			WorkerDisconnected?.Invoke(this,
				new WorkerConnectionEventArgs {Identity = worker.Identity});
		}

		private void OnWorkerConnected(WorkerConnection worker)
		{
			AssertConsumerThread();
			WorkerConnected?.Invoke(this,
				new WorkerConnectionEventArgs {Identity = worker.Identity});
		}

		private void OnHeartbeatTimeout(WorkerConnection worker)
		{
			AssertSocketThread();
			if (--worker.Liveness == 0)
			{
				Logger.LogWarning($"[{Identity}] - Worker '{worker.Identity}' is dead");
				RemoveWorker(worker);
			}
			else if (worker.Liveness < HeartbeatConfig.Liveness - 1)
			{
				Logger.LogWarning(
					$"[{Identity}] - Worker '{worker.Identity}' heartbeat timeout liveness={worker.Liveness}");
			}
		}

		private void SendHeartbeat(WorkerConnection worker)
		{
			DirectSend(CreateMessage(worker.Identity, null));
		}

		private void RemoveWorker(WorkerConnection worker)
		{
			AssertSocketThread();
			workers.Remove(worker.Identity);
			worker.IncomingHeartbeatTimer.Enable = false;
			worker.OutgoingHeartbeatTimer.Enable = false;

			// HACK: Currently cannot remove timer from NetMQPoller during Elapsed call.
			// queue the removal to other part of poll cycle, see ClearWorkerTimers() method
			EnqueueSocketTask(() => SocketPoller.Remove(worker.IncomingHeartbeatTimer));
			EnqueueSocketTask(() => SocketPoller.Remove(worker.OutgoingHeartbeatTimer));

			// inform about the disconnection
			EnqueueConsumerTask(() => OnWorkerDisconnected(worker));
		}

		private WorkerConnection NewWorker(string identity)
		{
			AssertSocketThread();
			var entry = new WorkerConnection(identity, HeartbeatConfig.HeartbeatInterval);
			entry.Liveness = HeartbeatConfig.Liveness;
			workers[identity] = entry;
			SocketPoller.Add(entry.IncomingHeartbeatTimer);
			SocketPoller.Add(entry.OutgoingHeartbeatTimer);
			entry.IncomingHeartbeatTimer.Elapsed += (_, a) => OnHeartbeatTimeout(entry);
			entry.OutgoingHeartbeatTimer.Elapsed +=
				(_, a) => SendHeartbeat(entry);
			SendHeartbeat(entry);
			return entry;
		}

		private class WorkerConnection
		{
			public WorkerConnection(string identity, int pingInterval)
			{
				Identity = identity;
				IncomingHeartbeatTimer = new NetMQTimer(pingInterval);
				OutgoingHeartbeatTimer = new NetMQTimer(pingInterval);
			}

			public NetMQTimer IncomingHeartbeatTimer { get; }
			public NetMQTimer OutgoingHeartbeatTimer { get; }

			public int Liveness { get; set; }

			public string Identity { get; }
		}
	}
}