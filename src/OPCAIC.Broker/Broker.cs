using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using OPCAIC.Common;
using OPCAIC.Messaging;
using OPCAIC.Messaging.Messages;
using OPCAIC.Utils;

namespace OPCAIC.Broker
{
	/// <inheritdoc cref="IBroker" />
	public class Broker : IBroker, IHostedService
	{
		private readonly ITimeService time;
		private readonly IBrokerConnector connector;
		private readonly Dictionary<string, WorkerEntry> workers;
		private readonly ILogger logger;
		private readonly SortedSet<WorkItem> taskQueue;
		private readonly BrokerOptions options;
		private Thread consumerThread;
		private bool shuttingDown;

		private Thread socketThread;

		public Broker(IBrokerConnector connector, ILogger<Broker> logger, IOptions<BrokerOptions> options, ITimeService time)
		{
			shuttingDown = false;
			this.options = options.Value;
			this.connector = connector;
			this.logger = logger;
			this.time = time;
			workers = new Dictionary<string, WorkerEntry>();
			taskQueue = new SortedSet<WorkItem>();
			connector.RegisterTimer(new TimedCallback(CleanupQueue, TimeSpan.FromSeconds(1)));
			RegisterHandlers();
		}

		/// <inheritdoc />
		public Task<BrokerStats> GetStats()
		{
			return Schedule(() =>
			{
				return new BrokerStats
				{
					Workers = workers.Values.Select(w => new WorkerInfo
					{
						Identity = w.Identity,
						CurrentJob = w.CurrentWorkItem?.Payload.JobId,
						Games = w.Capabilities?.SupportedGames,
						Stats = w.Stats == null ? null : new WorkerStats
						{
							Timestamp =	w.StatsReceived,
							AllocatedBytes = w.Stats.AllocatedBytes,
							Gen0Collections = w.Stats.Gen0Collections,
							Gen1Collections = w.Stats.Gen1Collections,
							Gen2Collections = w.Stats.Gen2Collections,
							DiskSpaceLeft = w.Stats.DiskSpaceLeft,
							DiskSpace = w.Stats.DiskSpace
						}
					}).ToList(),
					Games = GetGames(),
					JobCount = taskQueue.Count
				};
			});
		}

		/// <inheritdoc />
		public event EventHandler<WorkMessageBase> MessageExpired;

		private ICollection<string> GetGames()
		{
			return workers.Where(w => w.Value.Capabilities != null)
				.SelectMany(w => w.Value.Capabilities.SupportedGames).ToHashSet();
		}

		/// <inheritdoc />
		public Task EnqueueWork(WorkMessageBase msg)
		{
			return EnqueueWork(msg, time.Now);
		}

		/// <inheritdoc />
		public Task<bool> PrioritizeWork(Guid id)
		{
			return Schedule(() =>
			{
				var workItem = taskQueue.SingleOrDefault(wi => wi.Payload.JobId == id);
				if (workItem != null)
				{
					taskQueue.Remove(workItem);
					workItem.QueuedTime = DateTime.MinValue;
					taskQueue.Add(workItem);
					return true;
				}

				return false;
			});
		}

		public Task<List<WorkItem>> GetWorkItems()
		{
			return Schedule(taskQueue.Select(i => (WorkItem)i.Clone()).ToList);
		}

		/// <inheritdoc />
		public Task<bool> CancelWork(Guid id)
		{
			return Schedule(() =>
			{
				taskQueue.RemoveWhere(w => w.Payload.JobId == id);
				var worker = workers.Values.SingleOrDefault(w => w.CurrentWorkItem?.Payload.JobId == id);
				if (worker != null)
				{
					CancelWork(worker);
				}

				return true;
			});
		}

		/// <inheritdoc />
		public Task StartBrokering()
		{
			shuttingDown = false;
			SetupThreads();
			socketThread.Start();
			consumerThread.Start();
			return Task.CompletedTask;
		}

		/// <inheritdoc />
		public void RegisterHandler<TMessage>(Func<TMessage, Task> handler)
		{
			RegisterInternalHandler<TMessage>((_, msg) => handler(msg));
		}

		/// <inheritdoc />
		public Task<int> GetUnfinishedTasksCount()
		{
			return Schedule(() =>
				workers.Values.Count(w => w.CurrentWorkItem != null) +
				taskQueue.Count);
		}

		/// <inheritdoc />
		public async Task StopBrokering(CancellationToken cancellationToken)
		{
			shuttingDown = true;

			// make sure no worker is running anything
			await Schedule(() =>
			{
				foreach (var worker in workers.Values.Where(w => w.CurrentWorkItem != null))
				{
					CancelWork(worker);
				}
			});

			// wait until all workers report stopped or grace period expires
			while (!cancellationToken.IsCancellationRequested && await IsExecutingAnything())
			{
				await Task.Delay(1, cancellationToken);
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
		public async Task StartAsync(CancellationToken cancellationToken)
		{
			logger.LogInformation("Starting Broker");
			await StartBrokering();
			logger.LogInformation("Broker started");
		}

		/// <inheritdoc />
		public async Task StopAsync(CancellationToken cancellationToken)
		{
			logger.LogInformation("Stopping Broker");
			await StopBrokering(cancellationToken);
			logger.LogInformation("Broker stopped");
		}

		/// <inheritdoc />
		public Task EnqueueWork(WorkMessageBase msg, DateTime queueTime)
		{
			Require.ArgNotNull(msg, nameof(msg));

			return Schedule(() =>
			{
				var work = new WorkItem
				{
					Payload = msg,
					QueuedTime = queueTime,
					ExpirationTime = time.Now.AddSeconds(options.TaskRetentionSeconds)
				};

				// dispatch if some worker is available
				var worker = workers.Values.Where(w => CanWorkerExecute(w, msg)).FirstOrDefault(w => w.CurrentWorkItem == null);
				if (worker != null)
				{
					AssignWork(worker, work);
				}
				else
				{
					// will be dispatched later
					taskQueue.Add(work);
				}
			});
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
		private Task<bool> IsExecutingAnything()
		{
			return Schedule(() => { return workers.Values.Any(w => w.CurrentWorkItem != null); });
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

			var work = SelectWork(worker);
			if (work == null)
			{
				// nothing to be done
				return;
			}

			AssignWork(worker, work);
		}

		private void AssignWork(WorkerEntry worker, WorkItem work)
		{
			logger.LogInformation($"Dispatching work to {worker.Identity}");
			worker.CurrentWorkItem = work;
			Send(worker, work.Payload);
		}

		private WorkItem SelectWork(WorkerEntry worker)
		{
			var work = taskQueue.FirstOrDefault(i => CanWorkerExecute(worker, i.Payload));
			taskQueue.Remove(work);
			return work;
		}

		private void CleanupQueue()
		{
			var now = time.Now;

			var games = GetGames();
			var newExpire = now.AddSeconds(options.TaskRetentionSeconds);
			var removed = new List<WorkMessageBase>();

			taskQueue.RemoveWhere(w =>
			{
				if (games.Contains(w.Payload.GameKey))
				{
					w.ExpirationTime = newExpire;
				}
				else if ((now - w.QueuedTime).TotalSeconds > options.TaskRetentionSeconds)
				{
					removed.Add(w.Payload);
					return true;
				}

				return false;
			});

			foreach (var msg in removed)
			{
				MessageExpired?.Invoke(this, msg);
			}
		}

		private void RegisterHandlers()
		{
			// connectivity events
			connector.WorkerConnected += (_, a) => OnWorkerConnected(a.Identity);
			connector.WorkerDisconnected += (_, a) => OnWorkerDisconnected(a.Identity);
			connector.ReceivedMessage += (_, a) => OnMessageReceived(a.Sender, a.Payload);

			// message handlers
			RegisterInternalHandler<WorkerCapabilitiesMessage>(OnCapabilitiesReceived);
			RegisterInternalHandler<WorkerStatsReport>(OnWorkerStatsReport);
		}

		private Task OnWorkerStatsReport(WorkerEntry worker, WorkerStatsReport stats)
		{
			worker.Stats = stats;
			return Task.CompletedTask;
		}

		private void OnMessageReceived(string sender, object message)
		{
			switch (message)
			{
				case WorkerCapabilitiesMessage _:
				case WorkerStatsReport _:
				case ReplyMessageBase msg when msg.JobStatus == JobStatus.Canceled:
					return; // no more reaction needed
			}

			if (!workers.TryGetValue(sender, out var worker))
			{
				logger.LogError($"Ignoring message from unknown worker {sender}");
				return;
			}

			if (worker.CurrentWorkItem == null)
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

		private void RegisterInternalHandler<TMessage>(Func<WorkerEntry, TMessage, Task> handler)
		{
			connector.RegisterAsyncHandler<TMessage>(async (identity, message) =>
			{
				if (!workers.TryGetValue(identity, out var worker))
				{
					return; // ignore, unknown workers are handled elsewhere
				}

				try
				{
					await handler(worker, message);
				}
				catch (Exception e)
				{
					logger.LogError(e, "Exception occured when processing message:\n{@Message}",
						message);
				}
			});
		}

		private Task OnCapabilitiesReceived(WorkerEntry worker, WorkerCapabilitiesMessage msg)
		{
			logger.LogInformation($"Worker capabilities received from '{worker.Identity}' {{@{LoggingTags.WorkerCapabilities}}}", msg.Capabilities);
			worker.Capabilities = msg.Capabilities;

			if (worker.CurrentWorkItem == null)
			{
				DispatchWork(worker);
			}
			return Task.CompletedTask;
		}

		private void OnWorkerConnected(string identity)
		{
			if (workers.ContainsKey(identity))
			{
				logger.LogError($"Worker {identity} already connected.");
				return;
			}

			var worker = new WorkerEntry(identity);
			workers.Add(identity, worker);
			SetHeartbeat(worker);

			// make sure worker executes something
			DispatchWork(worker);
		}

		private void SetHeartbeat(WorkerEntry worker)
		{
			logger.LogInformation($"Sending heartbeat config to {worker.Identity}");
			CancelWork(worker);
			Send(worker, new SetConfigMessage
			{
				HeartbeatConfig = connector.HeartbeatConfig,
				ReportPeriod = TimeSpan.FromSeconds(10)
			});
		}

		private static bool CanWorkerExecute(WorkerEntry worker, WorkMessageBase msg)
		{
			return worker.Capabilities?.SupportedGames.Contains(msg.GameKey) == true;
		}

		private void OnWorkerDisconnected(string identity)
		{
			if (!workers.Remove(identity, out var worker))
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