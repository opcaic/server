using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using OPCAIC.Common;
using OPCAIC.Messaging;
using OPCAIC.Messaging.Messages;
using OPCAIC.Worker.Config;
using OPCAIC.Worker.GameModules;
using OPCAIC.Worker.Services;

namespace OPCAIC.Worker
{
	/// <summary>
	///     The main class of the opcaic worker process
	/// </summary>
	public class Worker : IHostedService
	{
		private readonly IWorkerConnector connector;
		private readonly ExecutionConfig executionConfig;
		private readonly ILogger logger;

		private readonly IServiceProvider serviceProvider;
		private Thread consumerThread;

		private CancellationTokenSource currentTaskCts;

		private TimedCallback reportCallback;

		private Thread socketThread;

		public Worker(IWorkerConnector connector, ILogger<Worker> logger,
			IServiceProvider serviceProvider, IOptions<ExecutionConfig> executionConfig,
			IGameModuleWatcher watcher)
		{
			this.connector = connector;
			this.logger = logger;
			this.serviceProvider = serviceProvider;
			this.executionConfig = executionConfig.Value;

			watcher.ModuleListChanged += ModuleListChanged;

			RegisterHandlers();
		}

		private string Identity => connector.Identity;

		/// <inheritdoc />
		public Task StartAsync(CancellationToken cancellationToken)
		{
			logger.LogInformation(LoggingEvents.Startup, "Starting Worker");
			SetupThreads();
			socketThread.Start();
			consumerThread.Start();
			InitConnection();
			logger.LogInformation(LoggingEvents.Startup, "Worker started");
			return Task.CompletedTask;
		}

		/// <inheritdoc />
		public Task StopAsync(CancellationToken cancellationToken)
		{
			logger.LogInformation(LoggingEvents.Startup, "Stopping Worker");
			connector.StopSocket();
			connector.StopConsumer();
			socketThread.Join();
			consumerThread.Join();
			logger.LogInformation(LoggingEvents.Startup, "Worker stopped");
			return Task.CompletedTask;
		}

		private void ModuleListChanged(object sender, EventArgs modules)
		{
			logger.LogInformation(LoggingEvents.GameModuleRefresh,
				"Refreshing list of game modules");
			SendCapabilities();
		}

		private void SetupThreads()
		{
			socketThread = new Thread(connector.EnterSocket) {Name = $"{Identity} - Socket"};
			consumerThread = new Thread(connector.EnterConsumer) {Name = $"{Identity} - Consumer"};
		}

		private void RegisterHandlers()
		{
			connector.RegisterAsyncHandler<MatchExecutionRequest>(
				ServeRequest<MatchExecutionRequest, MatchExecutionResult>);
			connector.RegisterAsyncHandler<SubmissionValidationRequest>(
				ServeRequest<SubmissionValidationRequest, SubmissionValidationResult>);
			connector.RegisterHandler<SetConfigMessage>(SetConfig);
			connector.RegisterHandler<CancelTaskMessage>(CancelTask);

			connector.RegisterTimer(new TimedCallback(CleanupOldDirectories, TimeSpan.FromDays(1)));
		}

		private void CleanupOldDirectories()
		{
			try
			{
				serviceProvider.GetRequiredService<IExecutionServices>().DirectoryCleanup();
			}
			catch (Exception e)
			{
				logger.LogError(e, "Failed to perform periodic cleanup of old directories.");
				// swallow the possible exception
			}
		}

		private void SetConfig(SetConfigMessage msg)
		{
			logger.LogInformation(LoggingEvents.WorkerConnection, "Resetting heartbeat config.");
			connector.SetHeartbeatConfig(msg.HeartbeatConfig);

			// reregister reporting callback with new period
			if (reportCallback != null)
			{
				connector.UnregisterTimer(reportCallback);
			}
			reportCallback = new TimedCallback(SendReport, msg.ReportPeriod, true);
			connector.RegisterTimer(reportCallback);

			// resend worker capabilities, this makes sure the capabilities are not forgotten when reconnecting after timeout
			SendCapabilities();
		}

		private void SendReport()
		{
			var driveInfo = new DriveInfo(AppContext.BaseDirectory);
			connector.SendMessage(new WorkerStatsReport
			{
				AllocatedBytes = GC.GetTotalMemory(false),
				Gen0Collections = GC.CollectionCount(0),
				Gen1Collections = GC.CollectionCount(1),
				Gen2Collections = GC.CollectionCount(2),
				DiskSpace = driveInfo.TotalSize,
				DiskSpaceLeft = driveInfo.AvailableFreeSpace
			});
		}

		private void CancelTask(CancelTaskMessage _)
		{
			// claim ownership of the CTS
			var cts = Interlocked.Exchange(ref currentTaskCts, null);
			if (cts != null)
			{
				logger.LogInformation(LoggingEvents.JobAborted, "Aborting current task");
				cts.Cancel();
				cts.Dispose();
			}
		}

		private CancellationToken SetupCancellation()
		{
			// keep local (thread-safe) reference
			var cts = new CancellationTokenSource(executionConfig.MaxTaskTimeoutSeconds * 1000);
			var token = cts.Token;
			currentTaskCts = cts;
			return token;
		}

		/// <summary>
		///     Atomically releases current cancellation token source. Returns true if cancellation
		///     was requested by the broker.
		/// </summary>
		/// <returns></returns>
		private bool FinalizeCancellation()
		{
			// atomically dispose if it is still there
			var cts = Interlocked.Exchange(ref currentTaskCts, null);
			cts?.Dispose();
			return cts == null;
		}

		private async Task ServeRequest<TRequest, TResult>(TRequest request)
			where TResult : ReplyMessageBase, new() where TRequest : WorkMessageBase
		{
			using (logger.TaskScope(request))
			{
				Debug.Assert(currentTaskCts == null);

				// Make sure we always send non-null message
				var response = new TResult {JobId = request.JobId, JobStatus = JobStatus.Error};

				var token = SetupCancellation();

				try
				{
					using var scope = serviceProvider.CreateScope();

					var r = await scope.ServiceProvider
						.GetRequiredService<IJobExecutor<TRequest, TResult>>()
						.ExecuteAsync(request, token);

					if (r != null)
					{
						response = r;
					}
				}
				// log in when handler to preserve scopes.
				catch (Exception e) when (DoLogExecutionFailure(e, request))
				{
					response.Exception = e.Message;
					response.JobStatus = e is OperationCanceledException
						? JobStatus.Canceled
						: JobStatus.Error;
				}

				var forced = FinalizeCancellation();
				if (forced)
				{
					response.JobStatus = JobStatus.Canceled;
				}

				connector.SendMessage(response);
			}
		}

		private bool DoLogExecutionFailure(Exception e, object request)
		{
			logger.LogError(LoggingEvents.JobExecutionFailure, e,
				"Exception occured when processing message" +
				$"{{@{LoggingTags.JobPayload}}}", request);

			return true;
		}

		public void Run()
		{
			var t = new Thread(connector.EnterSocket);
			t.Start();

			InitConnection();
			connector.EnterConsumer(); // returns on worker exit

			connector.StopSocket();
			t.Join();
		}

		private void InitConnection()
		{
			logger.LogInformation(LoggingEvents.WorkerConnection, "Initiating connection");
			SendCapabilities();
		}

		private void SendCapabilities()
		{
			connector.SendMessage(new WorkerCapabilitiesMessage
			{
				Capabilities = new WorkerCapabilities
				{
					SupportedGames = serviceProvider.GetService<IGameModuleRegistry>()
						.GetAllModuleNames()
						.ToList()
				}
			});
		}
	}
}