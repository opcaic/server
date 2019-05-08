using System;
using System.Collections.Generic;
using NetMQ;
using NetMQ.Sockets;
using OPCAIC.Messaging.Commands;
using OPCAIC.Messaging.Messages;
using OPCAIC.Messaging.Utils;

namespace OPCAIC.Messaging
{
	public class BrokerConnector : ConnectorBase<RouterSocket, ReceivedMessage>
	{
		private readonly string address;
		private readonly Dictionary<string, WorkerConnection> workers;

		public BrokerConnector(string address, string identity, HeartbeatConfig config)
			: base(
				identity,
				new RouterSocketFactory(identity, address, true),
				new HandlerSet<ReceivedMessage>(msg => msg.Payload),
				config)
		{
			this.address = address;
			workers = new Dictionary<string, WorkerConnection>();
		}

		public event EventHandler<WorkerConnectionEventArgs> WorkerDisconnected;
		public event EventHandler<WorkerConnectionEventArgs> WorkerConnected;

		public void RegisterAsyncHandler<T>(Action<string, T> handler)
			=> AddHandler(new HandlerInfo<ReceivedMessage>(typeof(T),
				msg => handler(msg.Sender, (T) msg.Payload), false));

		public void RegisterHandler<T>(Action<string, T> handler)
			=> AddHandler(new HandlerInfo<ReceivedMessage>(typeof(T),
				msg => handler(msg.Sender, (T) msg.Payload), true));

		public void SendMessage<T>(string recipient, T payload)
			=> EnqueueSocketTask(() =>
			{
				// treat each message as a heartbeat
				if (workers.TryGetValue(Identity, out var entry))
					// the worker might have been removed
					entry.OutgoingHeartbeatTimer.EnableAndReset();

				DirectSend(CreateMessage(recipient, payload));
			});

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
				EnqueueWorkerTask(() => OnWorkerConnected(entry));
			}

			msg.Pop(); // empty frame
			if (msg.IsEmpty)
				return null; // heartbeat messsage

			return new ReceivedMessage(sender, MessageHelpers.DeserializeMessage(msg));
		}

		private NetMQMessage CreateMessage(string recipient, object payload)
		{
			var msg = new NetMQMessage(2);
			msg.AppendIdentity(recipient);
			msg.AppendEmptyFrame();
			if (payload != null)
				MessageHelpers.SerializeMessage(msg, payload);
			return msg;
		}

		private void ResetHeartbeat(WorkerConnection connection)
		{
			AssertSocketThread();
			connection.IncomingHeartbeatTimer.EnableAndReset();
			connection.Liveness = Config.Liveness;
		}

		private void OnWorkerDisconnected(WorkerConnection worker)
		{
			AssertWorkThread();
			WorkerDisconnected?.Invoke(this, new WorkerConnectionEventArgs
			{
				Identity = worker.Identity
			});
		}

		private void OnWorkerConnected(WorkerConnection worker)
		{
			AssertWorkThread();
			WorkerConnected?.Invoke(this, new WorkerConnectionEventArgs
			{
				Identity = worker.Identity
			});
		}

		private void OnHeartbeatTimeout(WorkerConnection worker)
		{
			AssertSocketThread();
			if (--worker.Liveness == 0)
			{
				Console.WriteLine($"[{Identity}] - Worker '{worker.Identity}' is dead");
				RemoveWorker(worker);
			}
			else if (worker.Liveness < Config.Liveness - 1)
			{
				Console.WriteLine($"[{Identity}] - Worker '{worker.Identity}' livenes={worker.Liveness}");
			}
		}

		private void SendHeartbeat(WorkerConnection worker)
			=> DirectSend(CreateMessage(worker.Identity, null));

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
			EnqueueWorkerTask(() => OnWorkerDisconnected(worker));
		}

		private WorkerConnection NewWorker(string identity)
		{
			AssertSocketThread();
			var entry = new WorkerConnection(identity, Config.HeartbeatInterval);
			entry.Liveness = Config.Liveness;
			workers[identity] = entry;
			SocketPoller.Add(entry.IncomingHeartbeatTimer);
			SocketPoller.Add(entry.OutgoingHeartbeatTimer);
			entry.IncomingHeartbeatTimer.Elapsed += (_, a) => OnHeartbeatTimeout(entry);
			entry.OutgoingHeartbeatTimer.Elapsed +=
				(_, a) => SendHeartbeat(entry);
			SendHeartbeat(entry);
			return entry;
		}

		protected class WorkerConnection
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
