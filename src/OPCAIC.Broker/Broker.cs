using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.ExceptionServices;
using System.Runtime.InteropServices.ComTypes;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using NetMQ;
using Newtonsoft.Json;
using OPCAIC.Broker.Runner;
using OPCAIC.Messaging;
using OPCAIC.Messaging.Messages;
using OPCAIC.Utils;

namespace OPCAIC.Broker
{
	public class Broker : IBroker, IDisposable
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

		public void EnqueueWork(WorkMessageBase msg)
		{
			Require.NotNull(msg, nameof(msg));

			Schedule(() =>
			{
				var capableWorkers = identityToWorker.Values.Where(w => CanWorkerExecute(w, msg));
				if (!capableWorkers.Any())
				{
					logger.LogError($"No worker can execute game {msg.Game}");
				}

				taskQueue.Add(new WorkItem()
				{
					Payload = msg,
					QueuedTime = DateTime.Now
				});

				// enqueue to worker with shortest queue
				var worker = capableWorkers.FirstOrDefault(w => w.CurrentWorkItem == null);
				if (worker != null)
				{
					DispatchWork(worker);
				}
			});
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

		public void RegisterHandler<TMessage>(Action<TMessage> handler)
		{
			RegisterInternalHandler<TMessage>((_, msg) => handler(msg));
		}

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
				ExceptionDispatchInfo.Throw(e.InnerExceptions[0]);
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
				ExceptionDispatchInfo.Throw(e.InnerExceptions[0]);
				return default(T); // unreachable
			}
		}

		private void Send(WorkerEntry worker, WorkMessageBase msg)
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

		private void RegisterHandlers()
		{
			// connectivity events
			connector.WorkerConnected += (_, a) => OnWorkerConnected(a.Identity);
			connector.WorkerDisconnected += (_, a) => OnWorkerDisconnected(a.Identity);
			connector.MessageReceived += (_, a) => OnMessageReceived(a.Sender, a.Payload);

			// message handlers
			RegisterInternalHandler<WorkerConnectMessage>(OnWorkerConnected);
		}

		private void OnMessageReceived(string sender, object message)
		{
			if (!identityToWorker.TryGetValue(sender, out var worker))
			{
				logger.LogError($"Ignoring message from unknown worker {sender}");
				return;
			}

			if (worker.CurrentWorkItem == null && !(message is WorkerConnectMessage))
			{
				logger.LogError($"Received unexpected result message from '{worker.Identity}'");
			}
			else
			{
				worker.CurrentWorkItem = null;
				logger.LogInformation($"Worker {worker.Identity} finished.");
			}

			DispatchWork(worker);
		}

		private void RegisterInternalHandler<TMessage>(Action<WorkerEntry, TMessage> handler)
			=> connector.RegisterAsyncHandler<TMessage>((identity, message) =>
			{
				if (!identityToWorker.TryGetValue(identity, out var worker))
				{
					return; // ignore
				}

				try
				{
					handler(worker, message);
				}
				catch (Exception e)
				{
					logger.LogError(e, "Exception occured when processing message:\n{message}",
						JsonConvert.SerializeObject(message));
				}
			});

		private void OnWorkerConnected(WorkerEntry worker, WorkerConnectMessage msg)
		{
			worker.Capabilities = msg.Capabilities;
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

		private static bool CanWorkerExecute(WorkerEntry worker, WorkMessageBase msg)
			=> worker.Capabilities?.SupportedGames.Contains(msg.Game) == true;

		private void OnWorkerDisconnected(string identity)
		{
			if (!identityToWorker.Remove(identity, out var worker))
			{
				logger.LogError($"Trying to disconnect unconnected worker {identity}");
				return;
			}

			logger.LogInformation($"Worker {identity} disconnected");
			if (worker.CurrentWorkItem != null)
			{
				logger.LogInformation($"Requeuing {identity}'s work item");
				// return back go queue (with original enqueue time)
				taskQueue.Add(worker.CurrentWorkItem);
			}
		}
	}
}
