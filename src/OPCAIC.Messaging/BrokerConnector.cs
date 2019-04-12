using System;
using System.Collections.Generic;
using System.Text;
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
		private readonly Dictionary<string, WorkerEntry> workers;

		private readonly List<NetMQTimer> workersToRemove;

		public BrokerConnector(string address, string identity)
			: base(
				identity,
				new RouterSocketFactory(identity, address, true),
				new HandlerSet<ReceivedMessage>(msg => msg.Payload))
		{
			this.address = address;
			workers = new Dictionary<string, WorkerEntry>();
			workersToRemove = new List<NetMQTimer>();
		}

		private void OnPingTimeout(WorkerEntry worker)
		{
			if (--worker.Liveness == 0)
			{
				Console.WriteLine($"[{Identity}] - Worker '{worker.Identity}' is dead");
				RemoveWorker(worker);
				return;
			}

//			Console.WriteLine($"[{Identity}] - Sending ping to worker '{worker.Identity}'.");
			PingWorker(worker);
		}

		private void PingWorker(WorkerEntry worker)
			=> DirectSend(CreateMessage(worker.Identity, new PingMessage()));

		private void RemoveWorker(WorkerEntry worker)
		{
			workers.Remove(worker.Identity);
			// HACK: Currently cannot remove timer from NetMQPoller during Elapsed call.
			// queue the removal to other part of poll cycle, see ClearWorkerTimers() method
			worker.PingTimer.Enable = false;
			workersToRemove.Add(worker.PingTimer);
		}

		protected override ReceivedMessage ReceiveMessage(NetMQMessage msg)
		{
			ClearWorkerTimers();

			var sender = msg.Pop().ConvertToIdentity();
			if (msg.First.IsEmpty)
				msg.Pop();
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

		private static void ResetHeartbeat(WorkerEntry entry)
		{
			entry.PingTimer.EnableAndReset();
			entry.Liveness = Defaults.Liveness;
		}

		private void ClearWorkerTimers()
		{
			foreach (var timer in workersToRemove)
				SocketPoller.Remove(timer);

			workersToRemove.Clear();
		}

		private WorkerEntry NewWorker(string identity)
		{
			var entry = new WorkerEntry(identity);
			workers[identity] = entry;
			SocketPoller.Add(entry.PingTimer);
			entry.PingTimer.Elapsed += (_, a) => OnPingTimeout(entry);
			PingWorker(entry);
			return entry;
		}

		public void SendMessage<T>(string recipient, T payload)
			=> EnqueueMessage(CreateMessage(recipient, payload));

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
	}
}
