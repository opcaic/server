using System;
using System.Collections.Generic;
using System.Linq;
using OPCAIC.Messaging;
using OPCAIC.Messaging.Messages;

namespace OPCAIC.Broker.Runner
{
	public class Broker : IDisposable
	{
		private readonly BrokerConnector connector;
		private readonly Dictionary<string, WorkerEntry> workers;
		private readonly List<WorkerEntry> workerQueue;

		public Broker(BrokerConnector connector)
		{
			this.connector = connector;
			workers = new Dictionary<string, WorkerEntry>();
			workerQueue = new List<WorkerEntry>();
			RegisterHandlers();
		}

		private WorkerEntry PickCapableWorker(ExecuteMatchMessage msg)
		{
			foreach (var worker in workerQueue)
			{
				if (worker.CurrentWorkItem == null && // only free workers
					worker.HasGame(msg.Game) && 
					worker.HasRuntimes(msg.Bots.Select(b => b.Runtime)))
				{
					// remove worker from the queue
					workerQueue.Remove(worker);
					return worker;
				}
			}

			return null;
		}

		public void StartBrokering()
		{
			connector.EnterPollerAsync();
			connector.EnterConsumerAsync();
		}

		public void StopBrokering()
		{
			connector.StopPoller();
			connector.StopConsumer();
		}

		private void RegisterHandlers()
		{
			// connectivity events
			connector.WorkerConnected += (_, a) => OnWorkerConnected(a.Identity);
			connector.WorkerDisconnected += (_, a) => OnWorkerDisconnected(a.Identity);

			// match execution
			connector.RegisterAsyncHandler<MatchExecutionResultMessage>(OnMatchCompleted);

			// misc
			connector.RegisterAsyncHandler<RefuseMessage>(OnMatchRefused);
		}

		private void OnMatchRefused(string identity, RefuseMessage msg)
		{
			throw new NotImplementedException();
		}

		private void OnMatchCompleted(string identity, MatchExecutionResultMessage msg)
		{
			throw new NotImplementedException();
		}
		private void OnWorkerConnected(string identity, WorkerConnectMessage msg)
		{

		}

		private void OnWorkerConnected(string identity)
		{
			var worker = new WorkerEntry(identity);
			workers.Add(identity, worker);
			workerQueue.Add(worker);
		}

		private void OnWorkerDisconnected(string identity)
		{
			if (!workers.Remove(identity, out var worker))
			{
				Console.WriteLine("Error, trying to disconnect unconnected worker");
				return;
			}

			Console.WriteLine($"Worker {identity} disconnected");

			if (worker.CurrentWorkItem != null)
			{
				throw new NotImplementedException();
			}
		}

		public void Dispose() => connector.Dispose();
	}
}
