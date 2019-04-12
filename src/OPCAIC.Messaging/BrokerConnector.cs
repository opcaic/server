using System;
using System.Collections.Generic;
using NetMQ;
using NetMQ.Sockets;
using OPCAIC.Messaging.Commands;
using OPCAIC.Messaging.Messages;
using OPCAIC.Messaging.Utils;

namespace OPCAIC.Messaging
{
	public class WorkerDisconnectedEventArgs
	{
		public string Identity { get; set; }
	}

	public class BrokerConnector : ConnectorBase<RouterSocket, ReceivedMessage>
	{
		private readonly string address;

		private readonly Dictionary<string, WorkerConnection> workers;

		public BrokerConnector(string address, string identity)
			: base(
				identity,
				new RouterSocketFactory(identity, address, true),
				new HandlerSet<ReceivedMessage>(msg => msg.Payload))
		{
			this.address = address;
			workers = new Dictionary<string, WorkerConnection>();
		}

		public event EventHandler<WorkerDisconnectedEventArgs> WorkerDisconnected;

		private void OnPingTimeout(WorkerConnection worker)
		{
			if (--worker.Liveness == 0)
			{
				Console.WriteLine($"[{Identity}] - Worker '{worker.Identity}' is dead");
				RemoveWorker(worker);
				return;
			}

			PingWorker(worker);
		}

		private void PingWorker(WorkerConnection worker)
			=> DirectSend(CreateMessage(worker.Identity, new PingMessage()));

		private void RemoveWorker(WorkerConnection worker)
		{
			workers.Remove(worker.Identity);
			worker.PingTimer.Enable = false;
			// HACK: Currently cannot remove timer from NetMQPoller during Elapsed call.
			// queue the removal to other part of poll cycle, see ClearWorkerTimers() method
			EnqueueSocketTask(() => SocketPoller.Remove(worker.PingTimer));
			// inform about the disconnection
			EnqueueWorkerTask(() => OnWorkerDisconnected(worker));
		}

		protected override ReceivedMessage ReceiveMessage(NetMQMessage msg)
		{
			var sender = msg.Pop().ConvertToIdentity();
			if (msg.First.IsEmpty)
			{
				msg.Pop();
			}

			var payload = MessageHelpers.DeserializeMessage(msg);

			if (workers.TryGetValue(sender, out var entry))
			{
				// treat each message as a ping message
				ResetHeartbeat(entry);
			}
			else
			{
				Console.WriteLine($"[{Identity}] - new worker connected: {sender}");
				entry = NewWorker(sender);
			}

			return new ReceivedMessage(sender, payload);
		}

		private static void ResetHeartbeat(WorkerConnection connection)
		{
			connection.PingTimer.EnableAndReset();
			connection.Liveness = Defaults.Liveness;
		}

		private WorkerConnection NewWorker(string identity)
		{
			var entry = new WorkerConnection(identity);
			workers[identity] = entry;
			SocketPoller.Add(entry.PingTimer);
			entry.PingTimer.Elapsed += (_, a) => OnPingTimeout(entry);
			PingWorker(entry);
			return entry;
		}

		public void SendMessage<T>(string recipient, T payload)
			=> EnqueueSocketTask(() => DirectSend(CreateMessage(recipient, payload)));

		private NetMQMessage CreateMessage<T>(string recipient, T payload)
		{
			var msg = new NetMQMessage(3);
			msg.AppendIdentity(recipient);
			msg.AppendEmptyFrame();
			MessageHelpers.SerializeMessage(msg, payload);
			return msg;
		}

		public void RegisterHandler<T>(Action<string, T> handler)
			=> AddHandler(new HandlerInfo<ReceivedMessage>(typeof(T),
				msg => handler(msg.Sender, (T) msg.Payload), true));

		public void RegisterAsyncHandler<T>(Action<string, T> handler)
			=> AddHandler(new HandlerInfo<ReceivedMessage>(typeof(T),
				msg => handler(msg.Sender, (T) msg.Payload), false));

		protected virtual void OnWorkerDisconnected(WorkerConnection worker) => WorkerDisconnected?.Invoke(this, new WorkerDisconnectedEventArgs
		{
			Identity = worker.Identity
		});

		protected class WorkerConnection
		{
			public WorkerConnection(string identity)
			{
				Identity = identity;
				PingTimer = new NetMQTimer(Defaults.HeartbeatInterval);
				Liveness = Defaults.Liveness;
			}

			public NetMQTimer PingTimer { get; }

			public int Liveness { get; set; }

			public string Identity { get; }
		}
	}
}
