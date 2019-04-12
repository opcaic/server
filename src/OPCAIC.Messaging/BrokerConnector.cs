using System;
using System.Collections.Generic;
using System.Text;
using NetMQ;
using NetMQ.Sockets;
using OPCAIC.Messaging.Messages;

namespace OPCAIC.Messaging
{
	public class HandlerInfo<THandler>
	{
		public HandlerInfo(THandler handler, bool isSync)
		{
			Handler = handler;
			IsSync = isSync;
		}

		public THandler Handler { get; }
		public bool IsSync { get; }
	}
	public class BrokerConnector : ConnectorBase<RouterSocket>
	{
		private readonly string address;
		private readonly Dictionary<Type, HandlerInfo<Action<string, object>>> handlers;
		private readonly Dictionary<string, WorkerEntry> workers;

		private readonly List<NetMQTimer> workersToRemove;

		public BrokerConnector(string address, string identity)
			: base(new RouterSocketFactory(identity, address, true), identity)
		{
			this.address = address;
			handlers = new Dictionary<Type, HandlerInfo<Action<string, object>>>
			{
				[typeof(PingMessage)] = new HandlerInfo<Action<string, object>>(delegate { }, true) // ignore ping messages
			};
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

			Console.WriteLine($"[{Identity}] - Sending ping to worker '{worker.Identity}'.");
			PingWorker(worker);
		}

		private void PingWorker(WorkerEntry worker)
			=> Socket.SendMultipartMessage(CreateMessage(worker.Identity, new PingMessage()));

		private void RemoveWorker(WorkerEntry worker)
		{
			workers.Remove(worker.Identity);
			// HACK: Currently cannot remove timer from NetMQPoller during Elapsed call.
			// queue the removal to other part of poll cycle, see ClearWorkerTimers() method
			worker.PingTimer.Enable = false;
			workersToRemove.Add(worker.PingTimer);
		}

		protected override void OnPollerReceive(NetMQMessage msg)
		{
			ClearWorkerTimers();
			var sender = msg.First.ConvertToString(Encoding.Unicode);

			if (workers.TryGetValue(sender, out var entry))
			{
				// treat each message as a ping message
				entry.PingTimer.EnableAndReset();
				entry.Liveness = Defaults.Liveness;
			}
			else
			{
				// new worker connected
				Console.WriteLine($"[{Identity}] - new worker connected: {sender}");
				entry = NewWorker(sender);
			}

			base.OnPollerReceive(msg);
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
			return entry;
		}

		protected override void OnMessage(NetMQMessage msg)
		{
			var sender = msg.Pop().ConvertToString(Encoding.Unicode);
			var payload = DeserializeMessage(msg);
			if (!handlers.TryGetValue(payload.GetType(), out var handler))
			{
				throw new InvalidOperationException("Unknown payload type");
			}

			handler.Handler(sender, payload);
		}

		public void SendMessage<T>(string recipient, T payload)
		{
			var msg = CreateMessage(recipient, payload);
			EnqueueMessage(msg);
		}

		private NetMQMessage CreateMessage<T>(string recipient, T payload)
		{
			var msg = new NetMQMessage(3);
			msg.Append(recipient, Encoding.Unicode);
			SerializeMessage(msg, payload);
			return msg;
		}

		public void RegisterHandler<T>(Action<string, T> handler)
			=> handlers.Add(typeof(T), new HandlerInfo<Action<string, object>>((s, obj) => handler(s, (T) obj), true));
		public void RegisterAsyncHandler<T>(Action<string, T> handler)
			=> handlers.Add(typeof(T), new HandlerInfo<Action<string, object>>((s, obj) => handler(s, (T) obj), false));
	}
}
