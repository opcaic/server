using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using OPCAIC.Utils;
using OPCAIC.Messaging;
using OPCAIC.Messaging.Messages;

namespace OPCAIC.Broker.Runner
{
	public class Broker : IDisposable
	{
		private readonly BrokerConnector connector;
		private readonly Dictionary<string, WorkerEntry> workers;
		private readonly List<WorkerEntry> workerQueue;
		private readonly Queue<ExecuteMatchMessage> priorityTaskQueue;

		public Broker(BrokerConnector connector)
		{
			this.connector = connector;
			workers = new Dictionary<string, WorkerEntry>();
			workerQueue = new List<WorkerEntry>();
			priorityTaskQueue = new Queue<ExecuteMatchMessage>();
			RegisterHandlers();
		}

		public Task EnqueueMatchExecution(ExecuteMatchMessage msg)
		{
			return connector.EnqueueTask(() =>
			{
				var capableWorkers = workers.Values.Where(w => CanWorkerExecute(w, msg));
				if (!capableWorkers.Any())
				{
					Console.WriteLine($"Error: No worker can execute game {msg.Game}");
					priorityTaskQueue.Enqueue(msg);
					return;
				}

				// enqueue to worker with shortest queue
				var worker = capableWorkers.ArgMin(w => w.TaskQueue.Count);
				worker.TaskQueue.Enqueue(msg);
				if (worker.CurrentWorkItem == null)
					DispatchWork(worker);
			});
		}

		private void Send<TMessage>(WorkerEntry worker, TMessage msg)
		{
			connector.SendMessage(worker.Identity, msg);
		}

		private void DispatchWork(WorkerEntry worker)
		{
			if (worker.TaskQueue.Count == 0)
				return; // no work to be done
			Console.WriteLine($"Dispatching work to {worker.Identity}");
			var msg = worker.TaskQueue.Dequeue();
			worker.CurrentWorkItem = msg;
			Send(worker, msg);
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

			// worker capability
			RegisterHandler<WorkerConnectMessage>(OnWorkerConnected);

			// match execution
			RegisterHandler<MatchExecutionResultMessage>(OnMatchCompleted);

			// misc
			RegisterHandler<RefuseMessage>(OnMatchRefused);
		}

		private void RegisterHandler<TMessage>(Action<WorkerEntry, TMessage> handler)
		{
			connector.RegisterAsyncHandler<TMessage>((identity, message) =>
			{
				if (!workers.TryGetValue(identity, out var worker))
				{
					Console.WriteLine("Ignoring message from unknown worker");
					return;
				}

				handler(worker, message);
			});
		}

		private void OnMatchRefused(WorkerEntry worker, RefuseMessage msg)
		{
			throw new NotImplementedException();
		}

		private void OnMatchCompleted(WorkerEntry worker, MatchExecutionResultMessage msg)
		{
			worker.CurrentWorkItem = null;
			Console.WriteLine($"Worker {worker.Identity} finished.");
			DispatchWork(worker);
		}
		private void OnWorkerConnected(WorkerEntry worker, WorkerConnectMessage msg)
		{
			worker.Capabilities = msg.Capabilities;
		}

		private void OnWorkerConnected(string identity)
		{
			if (workers.ContainsKey(identity))
			{
				Console.WriteLine($"Error: worker {identity} already connected.");
				return;
			}

			var worker = new WorkerEntry(identity);
			workers.Add(identity, worker);
			workerQueue.Add(worker);
		}

		private static bool CanWorkerExecute(WorkerEntry worker, ExecuteMatchMessage msg)
		{
			return worker.Capabilities.SupportedGames.Contains(msg.Game);
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
				Console.WriteLine($"Requeuing {identity}'s work item");
				priorityTaskQueue.Enqueue(worker.CurrentWorkItem);
			}
		}

		public void Dispose() => connector.Dispose();
	}
}
