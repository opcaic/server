using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices.ComTypes;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using NetMQ;
using NetMQ.Sockets;

namespace OPCAIC.Messaging
{
	public static class Defaults
	{
		public static int PingTimeout => 1000;
		public static int WorkerLiveness => 3;
	}

	public class WorkerEntry
	{
		public WorkerEntry(string identity)
		{
			Identity = identity;
			PingTimer = new NetMQTimer(Defaults.PingTimeout);
			Liveness = Defaults.WorkerLiveness;
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

		public BrokerConnector(string address, string identity)
			:base (new RouterSocket(address), identity)
		{
			this.address = address;
			handlers = new Dictionary<Type, Action<string, object>>()
			{
				[typeof(PingMessage)] = delegate { } // ignore ping messages
			};
			workers = new Dictionary<string, WorkerEntry>();
		}

		void OnPingTimeout(WorkerEntry worker)
		{
			Console.WriteLine($"[{Identity}] - Worker '{worker.Identity}' ping timeout.");
			Socket.SendMultipartMessage(CreateMessage(worker.Identity, new PingMessage()));
		}

		protected override void OnPollerReceive(NetMQMessage msg)
		{
			string sender = msg.First.ConvertToString(Encoding.Unicode);

			if (!workers.TryGetValue(sender, out var entry))
			{
				// new worker connected
				entry =  NewWorker(sender);
			}

			// treat each message as a ping message
			entry.PingTimer.EnableAndReset();

			base.OnPollerReceive(msg);
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