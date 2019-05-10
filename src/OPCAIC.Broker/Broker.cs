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

		public event EventHandler<MatchExecutionResultMessage> MatchExecuted; 

		public Broker(BrokerConnector connector)
		{
			this.connector = connector;
			workers = new Dictionary<string, WorkerEntry>();
			workerQueue = new List<WorkerEntry>();
			priorityTaskQueue = new Queue<ExecuteMatchMessage>();
			RegisterHandlers();
		}

		public void EnqueueMatchExecution(ExecuteMatchMessage msg)
		{
			Schedule(() =>
			{
				var capableWorkers = workers.Values.Where(w => CanWorkerExecute(w, msg));
				if (!capableWorkers.Any())
				{
					priorityTaskQueue.Enqueue(msg);
					throw new InvalidOperationException($"Error: No worker can execute game {msg.Game}");
				}

				// enqueue to worker with shortest queue
				var worker = capableWorkers.ArgMin(w => w.TaskQueue.Count);
				worker.TaskQueue.Enqueue(msg);
				if (worker.CurrentWorkItem == null)
					DispatchWork(worker);
			});
		}

		private void Schedule(Action a)
		{
			Task task = new Task(a);
			connector.EnqueueTask(task);
			task.Wait();
		}

		private T Schedule<T>(Func<T> a)
		{
			var task = new Task<T>(a);
			connector.EnqueueTask(task);;
			return task.Result;
		}

		private void Send<TMessage>(WorkerEntry worker, TMessage msg)
		{
			connector.SendMessage(worker.Identity, msg);
		}

		private void DispatchWork(WorkerEntry worker)
		{
			Queue<ExecuteMatchMessage> src;
			if (priorityTaskQueue.Count > 0 && CanWorkerExecute(worker, priorityTaskQueue.Peek()))
				src = priorityTaskQueue;
			else
				src = worker.TaskQueue;

			if (src.Count == 0)
				return; // no work to be done

			Console.WriteLine($"Dispatching work to {worker.Identity}");
			var msg = src.Dequeue();
			worker.CurrentWorkItem = msg;
			Send(worker, msg);
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
			MatchExecuted?.Invoke(this, msg);
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

			Console.WriteLine($"Requeuing {identity}'s work items");
			if (worker.CurrentWorkItem != null)
				priorityTaskQueue.Enqueue(worker.CurrentWorkItem);

			foreach (var msg in worker.TaskQueue)
				priorityTaskQueue.Enqueue(msg);
		}

		public void Dispose() => connector.Dispose();
	}
}
