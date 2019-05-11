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
	internal class WorkItem : IComparable<WorkItem>
	{
		public DateTime QueuedTime { get; set; }
		public ExecuteMatchMessage Payload { get; set; }

		public int CompareTo(WorkItem other)
		{
			if (ReferenceEquals(this, other))
			{
				return 0;
			}

			if (ReferenceEquals(null, other))
			{
				return 1;
			}

			// order by time
			return QueuedTime.CompareTo(other.QueuedTime);
		}
	}

	public class Broker : IDisposable
	{
		private readonly BrokerConnector connector;
		private readonly ILogger logger;
		private readonly SortedSet<WorkItem> taskQueue;
		private readonly List<WorkerEntry> workers;
		private readonly Dictionary<string, WorkerEntry> identityToWorker;

		public Broker(BrokerConnector connector, ILogger<Broker> logger)
		{
			this.connector = connector;
			this.logger = logger;
			identityToWorker = new Dictionary<string, WorkerEntry>();
			workers = new List<WorkerEntry>();
			taskQueue = new SortedSet<WorkItem>();
			RegisterHandlers();
		}

		public void Dispose() => connector.Dispose();

		public event EventHandler<MatchExecutionResultMessage> MatchExecuted;

		public void EnqueueMatchExecution(ExecuteMatchMessage msg)
			=> Schedule(() =>
			{
				var capableWorkers = identityToWorker.Values.Where(w => CanWorkerExecute(w, msg));
				if (!capableWorkers.Any())
				{
					taskQueue.Add(new WorkItem()
					{
						Payload = msg,
						QueuedTime = DateTime.Now
					});
					logger.LogError($"No worker can execute game {msg.Game}");
				}

				// enqueue to worker with shortest queue
				var worker = capableWorkers.FirstOrDefault(w => w.CurrentWorkItem == null);
				if (worker != null)
				{
					DispatchWork(worker);
				}
			});

		public int GetUnfinishedTasksCount() => Schedule(() => 
			workers.Count(w => w.CurrentWorkItem != null) +
			taskQueue.Count);

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
			var work = taskQueue.FirstOrDefault(i => CanWorkerExecute(worker, i.Payload));
			if (work == null)
			{
				// nothing to be done
				return;
			}

			taskQueue.Remove(work);

			logger.LogInformation($"Dispatching work to {worker.Identity}");
			worker.CurrentWorkItem = work;
			Send(worker, work.Payload);
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
				if (!identityToWorker.TryGetValue(identity, out var worker))
				{
					logger.LogError($"Ignoring message from unknown worker {identity}");
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
		{
			worker.Capabilities = msg.Capabilities;
			DispatchWork(worker);
		}

		private void OnWorkerConnected(string identity)
		{
			if (identityToWorker.ContainsKey(identity))
			{
				logger.LogError($"Worker {identity} already connected.");
				return;
			}

			var worker = new WorkerEntry(identity);
			identityToWorker.Add(identity, worker);
			workers.Add(worker);
		}

		private static bool CanWorkerExecute(WorkerEntry worker, ExecuteMatchMessage msg)
			=> worker.Capabilities?.SupportedGames.Contains(msg.Game) == true;

		private void OnWorkerDisconnected(string identity)
		{
			if (!identityToWorker.Remove(identity, out var worker))
			{
				logger.LogError($"Trying to disconnect unconnected worker {identity}");
				return;
			}

			logger.LogInformation($"Worker {identity} disconnected");

			logger.LogInformation($"Requeuing {identity}'s work items");

			if (worker.CurrentWorkItem != null)
			{
				// return back go queue (with original enqueue time)
				taskQueue.Add(worker.CurrentWorkItem);
			}
		}
	}
}
