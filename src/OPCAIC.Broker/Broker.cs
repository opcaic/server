using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.ExceptionServices;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using OPCAIC.Messaging;
using OPCAIC.Messaging.Messages;
using OPCAIC.Utils;

namespace OPCAIC.Broker
{
	/// <inheritdoc cref="IBroker" />
	public class Broker : IBroker, IHostedService
	{
		private readonly IBrokerConnector connector;
		private readonly Dictionary<string, WorkerEntry> identityToWorker;
		private readonly ILogger logger;
		private readonly SortedSet<WorkItem> taskQueue;
		private readonly List<WorkerEntry> workers;
		private Thread consumerThread;
		private bool shuttingDown;

		private Thread socketThread;

		public Broker(IBrokerConnector connector, ILogger<Broker> logger)
		{
			shuttingDown = false;
			this.connector = connector;
			this.logger = logger;
			identityToWorker = new Dictionary<string, WorkerEntry>();
			workers = new List<WorkerEntry>();
			taskQueue = new SortedSet<WorkItem>();
			RegisterHandlers();
		}

		/// <inheritdoc />
		public Task<BrokerStatsDto> GetStats()
		{
			return Schedule(() => new BrokerStatsDto
			{
				Workers = workers.Select(w => new WorkerInfoDto
				{
					Identity = w.Identity, CurrentJob = w.CurrentWorkItem?.Payload.Id
				}).ToList()
			});
		}

		/// <inheritdoc />
		public Task EnqueueWork(WorkMessageBase msg)
		{
			return EnqueueWork(msg, DateTime.Now);
		}

		/// <inheritdoc />
		public Task EnqueueWork(WorkMessageBase msg, DateTime queueTime)
		{
			Require.ArgNotNull(msg, nameof(msg));

			return Schedule(() =>
			{
				var capableWorkers = identityToWorker.Values.Where(w => CanWorkerExecute(w, msg));
				if (!capableWorkers.Any())
				{
					logger.LogError($"No worker can execute game {msg.Game}");
				}

				taskQueue.Add(new WorkItem {Payload = msg, QueuedTime = queueTime});

				// enqueue to worker with shortest queue
				var worker = capableWorkers.FirstOrDefault(w => w.CurrentWorkItem == null);
				if (worker != null)
				{
					DispatchWork(worker);
				}
			});
		}

		/// <inheritdoc />
		public Task<bool> PrioritizeWork(Guid id)
		{
			return Schedule(() =>
			{
				if (taskQueue.All(wi => wi.Payload.Id != id)) return false;
				var workItem = taskQueue.Single(wi => wi.Payload.Id == id);
				taskQueue.Remove(workItem);
				workItem.QueuedTime = DateTime.MinValue;
				taskQueue.Add(workItem);
				return true;
			});
		}

		/// <inheritdoc />
		public Task<List<WorkItem>> FilterWork(WorkItemFilterDto filter)
		{
			return Schedule(() =>
				filter.Filter(taskQueue));
		}

		/// <inheritdoc />
		public Task<bool> CancelWork(Guid id)
		{
			return Schedule(() =>
			{
				taskQueue.RemoveWhere(w => w.Payload.Id == id);
				var worker = workers.SingleOrDefault(w => w.CurrentWorkItem?.Payload.Id == id);
				if (worker != null)
				{
					SetHeartbeat(worker);
				}
				return true;
			});
		}

		/// <inheritdoc />
		public Task<bool> CancelWork(string workerIdentity)
		{
			return Schedule(() =>
			{
				var worker = workers.SingleOrDefault(w => w.Identity == workerIdentity);
				if (worker != null)
				{
					SetHeartbeat(worker);
					return true;
				}
				return false;
			});
		}

		/// <inheritdoc />
		public void StartBrokering()
		{
			shuttingDown = false;
			SetupThreads();
			socketThread.Start();
			consumerThread.Start();
		}

		/// <inheritdoc />
		public void RegisterHandler<TMessage>(Action<TMessage> handler)
		{
			RegisterInternalHandler<TMessage>((_, msg) => handler(msg));
		}

		/// <inheritdoc />
		public int GetUnfinishedTasksCount()
		{
			return PerformTask(() =>
				workers.Count(w => w.CurrentWorkItem != null) +
				taskQueue.Count);
		}

		/// <inheritdoc />
		public void StopBrokering(CancellationToken cancellationToken)
		{
			shuttingDown = true;

			// make sure no worker is running anything
			PerformTask(() =>
			{
				foreach (var worker in workers.Where(w => w.CurrentWorkItem != null))
				{
					CancelWork(worker);
				}
			});

			var force = false;
			cancellationToken.Register(() => force = true);

			// wait until all workers report stopped or grace period expires
			while (!Volatile.Read(ref force) && IsExecutingAnything())
			{
				Thread.Sleep(1);
			}

			connector.StopSocket();
			connector.StopConsumer();
			if (!cancellationToken.IsCancellationRequested)
			{
				socketThread.Join();
				consumerThread.Join();
			}
			else
			{
				socketThread.Abort();
				consumerThread.Abort();
			}
		}

		/// <inheritdoc />
		public Task StartAsync(CancellationToken cancellationToken)
		{
			logger.LogInformation("Starting Broker");
			StartBrokering();
			logger.LogInformation("Broker started");

			return Task.CompletedTask;
		}

		/// <inheritdoc />
		public Task StopAsync(CancellationToken cancellationToken)
		{
			logger.LogInformation("Stopping Broker");
			StopBrokering(cancellationToken);
			logger.LogInformation("Broker stopped");

			return Task.CompletedTask;
		}

		private void SetupThreads()
		{
			socketThread =
				new Thread(connector.EnterSocket) {Name = $"{connector.Identity} - Socket"};
			consumerThread =
				new Thread(connector.EnterConsumer) {Name = $"{connector.Identity} - Consumer"};
		}

		/// <summary>
		///     Returns true if some worker is executing anything.
		/// </summary>
		/// <returns></returns>
		public bool IsExecutingAnything()
		{
			return PerformTask(() => { return workers.Any(w => w.CurrentWorkItem != null); });
		}

		/// <inheritdoc />
		public void ClearWorkQueue()
		{
			taskQueue.Clear();
		}

		/// <summary>
		///     Schedules an action to be invoked in a broker consumer thread and returns a <see cref="Task" /> object which can be
		///     awaited for completion.
		/// </summary>
		/// <param name="a"></param>
		/// <returns></returns>
		private Task Schedule(Action a)
		{
			var task = new Task(a);
			connector.EnqueueTask(task);
			return task;
		}

		/// <summary>
		///     Schedules an action to be invoked in a brokers consumer thread and waits for the completion.
		/// </summary>
		/// <param name="a">The action to be invoked.</param>
		private void PerformTask(Action a)
		{
			try
			{
				Schedule(a).Wait();
			}
			catch (AggregateException e)
			{
				ExceptionDispatchInfo.Throw(e.InnerExceptions[0]);
			}
		}

		/// <summary>
		///     Schedules an action to be invoked in a broker consumer thread and returns a <see cref="Task" /> object which can be
		///     awaited for completion.
		/// </summary>
		/// <param name="a"></param>
		/// <returns></returns>
		private Task<T> Schedule<T>(Func<T> a)
		{
			var task = new Task<T>(a);
			connector.EnqueueTask(task);
			return task;
		}

		/// <summary>
		///     Schedules a function to be invoked in a brokers consumer thread and waits for the completion.
		/// </summary>
		/// <param name="a">The function to be invoked.</param>
		/// <returns>The return value of of a()</returns>
		private T PerformTask<T>(Func<T> a)
		{
			try
			{
				return Schedule(a).Result;
			}
			catch (AggregateException e)
			{
				ExceptionDispatchInfo.Throw(e.InnerExceptions[0]);
				return default; // unreachable
			}
		}

		private void Send(WorkerEntry worker, object msg)
		{
			connector.SendMessage(worker.Identity, msg);
		}

		private void DispatchWork(WorkerEntry worker)
		{
			if (shuttingDown)
			{
				return;
			}

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
			connector.ReceivedMessage += (_, a) => OnMessageReceived(a.Sender, a.Payload);

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

			// try dispatching next work item for this worker
			DispatchWork(worker);
		}

		private void RegisterInternalHandler<TMessage>(Action<WorkerEntry, TMessage> handler)
		{
			connector.RegisterAsyncHandler<TMessage>((identity, message) =>
			{
				if (!identityToWorker.TryGetValue(identity, out var worker))
				{
					return; // ignore, unknown workers are handled elsewhere
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
		}

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
			SetHeartbeat(worker);

			// make sure worker executes something
			DispatchWork(worker);
		}

		private void SetHeartbeat(WorkerEntry worker)
		{
			logger.LogInformation($"Sending heartbeat config to {worker.Identity}");
			CancelWork(worker);
			Send(worker, new SetHeartbeatMessage {HeartbeatConfig = connector.HeartbeatConfig});
		}

		private static bool CanWorkerExecute(WorkerEntry worker, WorkMessageBase msg)
		{
			return worker.Capabilities?.SupportedGames.Contains(msg.Game) == true;
		}

		private void OnWorkerDisconnected(string identity)
		{
			if (!identityToWorker.Remove(identity, out var worker))
			{
				logger.LogError($"Trying to disconnect unconnected worker {identity}");
				return;
			}

			logger.LogInformation($"Worker {identity} disconnected");
			CancelWork(worker);
		}

		private void CancelWork(WorkerEntry worker)
		{
			Send(worker, new CancelTaskMessage());

			if (worker.CurrentWorkItem != null && !shuttingDown)
			{
				logger.LogInformation($"Requeueing {worker.Identity}'s work item");
				// return back go queue (with original enqueue time)
				taskQueue.Add(worker.CurrentWorkItem);
			}

			worker.CurrentWorkItem = null;
		}
	}
}