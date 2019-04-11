using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices.ComTypes;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using NetMQ;
using NetMQ.Sockets;
using OPCAIC.Messaging.Messages;

namespace OPCAIC.Messaging
{
	public static class Defaults
	{
		public static int HeartbeatInterval => 1000;
		public static int Liveness => 3;
		public static int ReconnectIntervalInit => 1000;
		public static int ReconnectIntervalMax => 32000;
	}

	public class WorkerEntry
	{
		public WorkerEntry(string identity)
		{
			Identity = identity;
			PingTimer = new NetMQTimer(Defaults.HeartbeatInterval);
			Liveness = Defaults.Liveness;
		}
		public NetMQTimer PingTimer { get; }
		public int Liveness { get; set; }

		public string Identity { get; }
	}

	public class BrokerConnector : ConnectorBase<RouterSocket>
	{
		private readonly string address;
		private readonly Dictionary<Type, Action<string, object>> handlers;
		private readonly Dictionary<string, WorkerEntry> workers;

		private readonly List<NetMQTimer> workersToRemove;

		public BrokerConnector(string address, string identity)
			:base (new RouterSocket(address), identity)
		{
			this.address = address;
			handlers = new Dictionary<Type, Action<string, object>>()
			{
				[typeof(PingMessage)] = delegate { } // ignore ping messages
			};
			workers = new Dictionary<string, WorkerEntry>();
			workersToRemove = new List<NetMQTimer>();
		}

		void OnPingTimeout(WorkerEntry worker)
		{
			if (--worker.Liveness == 0)
			{
				Console.WriteLine($"[{Identity}] - Worker '{worker.Identity}' is dead");
				RemoveWorker(worker);
				return;
			}
			Console.WriteLine($"[{Identity}] - Sending ping to worker '{worker.Identity}'.");
			Socket.SendMultipartMessage(CreateMessage(worker.Identity, new PingMessage()));
		}

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
			string sender = msg.First.ConvertToString(Encoding.Unicode);

			if (workers.TryGetValue(sender, out var entry))
			{
				// treat each message as a ping message
				entry.PingTimer.EnableAndReset();
				entry.Liveness = Defaults.Liveness;
			}
			else
			{
				// new worker connected
				entry =  NewWorker(sender);
			}

			base.OnPollerReceive(msg);
		}

		private void ClearWorkerTimers()
		{
			foreach (var timer in workersToRemove)
			{
				SocketPoller.Remove(timer);
			}

			workersToRemove.Clear();
		}

		WorkerEntry NewWorker(string identity)
		{
			var entry = new WorkerEntry(identity);
			workers[identity] = entry;
			SocketPoller.Add(entry.PingTimer);
			entry.PingTimer.Elapsed += (_, a) => OnPingTimeout(entry);
			return entry;
		}

		protected override void OnMessage(NetMQMessage msg)
		{
			string sender = msg.Pop().ConvertToString(Encoding.Unicode);
			object payload = DeserializeMessage(msg);
			if (!handlers.TryGetValue(payload.GetType(), out var handler))
			{
				throw new InvalidOperationException("Unknown payload type");
			}

			handler(sender, payload);
		}

		public void SendMessage<T>(string recipient, T payload)
		{
			var msg = CreateMessage(recipient, payload);
			EnqueueMessage(msg);
		}

		private NetMQMessage CreateMessage<T>(string recipient, T payload)
		{
			NetMQMessage msg = new NetMQMessage(3);
			msg.Append(recipient, Encoding.Unicode);
			SerializeMessage(msg, payload);
			return msg;
		}

		public void RegisterHandler<T>(Action<string, T> handler)
		{
			handlers.Add(typeof(T), (s, obj) =>handler(s, (T) obj));
		}
	}
}