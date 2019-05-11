using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.ExceptionServices;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using OPCAIC.Broker.Runner;
using OPCAIC.Messaging;
using OPCAIC.Messaging.Messages;
using OPCAIC.Utils;

namespace OPCAIC.Broker
{
	public class Broker : IDisposable
	{
		private readonly BrokerConnector connector;
		private readonly ILogger logger;
		private readonly Queue<ExecuteMatchMessage> priorityTaskQueue;
		private readonly List<WorkerEntry> workerQueue;
		private readonly Dictionary<string, WorkerEntry> workers;

		public Broker(BrokerConnector connector, ILogger<Broker> logger)
		{
			this.connector = connector;
			this.logger = logger;
			workers = new Dictionary<string, WorkerEntry>();
			workerQueue = new List<WorkerEntry>();
			priorityTaskQueue = new Queue<ExecuteMatchMessage>();
			RegisterHandlers();
		}

		public void Dispose() => connector.Dispose();

		public event EventHandler<MatchExecutionResultMessage> MatchExecuted;

		public void EnqueueMatchExecution(ExecuteMatchMessage msg)
			=> Schedule(() =>
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
				{
					DispatchWork(worker);
				}
			});

		private void Schedule(Action a)
		{
			var task = new Task(a);
			connector.EnqueueTask(task);
			try
			{
				task.Wait();
			}
			catch (AggregateException e)
			{
				ExceptionDispatchInfo.Capture(e.InnerExceptions[0]).Throw();
			}
		}

		private T Schedule<T>(Func<T> a)
		{
			var task = new Task<T>(a);
			connector.EnqueueTask(task);
			;
			try
			{
				return task.Result;
			}
			catch (AggregateException e)
			{
				ExceptionDispatchInfo.Capture(e.InnerExceptions[0]).Throw();
				return default(T); // unreachable
			}
		}

		private void Send<TMessage>(WorkerEntry worker, TMessage msg)
			=> connector.SendMessage(worker.Identity, msg);

		private void DispatchWork(WorkerEntry worker)
		{
			Queue<ExecuteMatchMessage> src;
			if (priorityTaskQueue.Count > 0 && CanWorkerExecute(worker, priorityTaskQueue.Peek()))
			{
				src = priorityTaskQueue;
			}
			else
			{
				src = worker.TaskQueue;
			}

			if (src.Count == 0)
			{
				return; // no work to be done
			}

			logger.LogInformation($"Dispatching work to {worker.Identity}");
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
			=> connector.RegisterAsyncHandler<TMessage>((identity, message) =>
			{
				if (!workers.TryGetValue(identity, out var worker))
				{
					logger.LogInformation("Ignoring message from unknown worker");
					return;
				}

				handler(worker, message);
			});

		private void OnMatchRefused(WorkerEntry worker, RefuseMessage msg)
			=> throw new NotImplementedException();

		private void OnMatchCompleted(WorkerEntry worker, MatchExecutionResultMessage msg)
		{
			worker.CurrentWorkItem = null;
			logger.LogInformation($"Worker {worker.Identity} finished.");
			MatchExecuted?.Invoke(this, msg);
			DispatchWork(worker);
		}

		private void OnWorkerConnected(WorkerEntry worker, WorkerConnectMessage msg)
			=> worker.Capabilities = msg.Capabilities;

		private void OnWorkerConnected(string identity)
		{
			if (workers.ContainsKey(identity))
			{
				logger.LogInformation($"Error: worker {identity} already connected.");
				return;
			}

			var worker = new WorkerEntry(identity);
			workers.Add(identity, worker);
			workerQueue.Add(worker);
		}

		private static bool CanWorkerExecute(WorkerEntry worker, ExecuteMatchMessage msg)
			=> worker.Capabilities?.SupportedGames.Contains(msg.Game) == true;

		private void OnWorkerDisconnected(string identity)
		{
			if (!workers.Remove(identity, out var worker))
			{
				logger.LogInformation("Error, trying to disconnect unconnected worker");
				return;
			}

			logger.LogInformation($"Worker {identity} disconnected");

			logger.LogInformation($"Requeuing {identity}'s work items");
			if (worker.CurrentWorkItem != null)
			{
				priorityTaskQueue.Enqueue(worker.CurrentWorkItem);
			}

			foreach (var msg in worker.TaskQueue)
				priorityTaskQueue.Enqueue(msg);
		}
	}
}
