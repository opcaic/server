﻿using System;
using System.Collections.Generic;
using System.Linq;
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
		public Task<BrokerStats> GetStats()
		{
			return Schedule(() => new BrokerStats
			{
				Workers = workers.Select(w => new WorkerInfo
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
		public Task<bool> PrioritizeWork(Guid id)
		{
			return Schedule(() =>
			{
				var workItem = taskQueue.SingleOrDefault(wi => wi.Payload.Id == id);
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
				taskQueue.RemoveWhere(w => w.Payload.Id == id);
				var worker = workers.SingleOrDefault(w => w.CurrentWorkItem?.Payload.Id == id);
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
		public void RegisterHandler<TMessage>(Action<TMessage> handler)
		{
			RegisterInternalHandler<TMessage>((_, msg) => handler(msg));
		}

		/// <inheritdoc />
		public Task<int> GetUnfinishedTasksCount()
		{
			return Schedule(() =>
				workers.Count(w => w.CurrentWorkItem != null) +
				taskQueue.Count);
		}

		/// <inheritdoc />
		public async Task StopBrokering(CancellationToken cancellationToken)
		{
			shuttingDown = true;

			// make sure no worker is running anything
			await Schedule(() =>
			{
				foreach (var worker in workers.Where(w => w.CurrentWorkItem != null))
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
		private Task EnqueueWork(WorkMessageBase msg, DateTime queueTime)
		{
			Require.ArgNotNull(msg, nameof(msg));

			return Schedule(() =>
			{
				var capableWorkers = identityToWorker.Values.Where(w => CanWorkerExecute(w, msg)).ToArray();
				if (capableWorkers.Length == 0)
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
			return Schedule(() => { return workers.Any(w => w.CurrentWorkItem != null); });
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