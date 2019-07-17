using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using OPCAIC.Messaging;
using OPCAIC.Messaging.Messages;
using OPCAIC.Worker.GameModules;
using OPCAIC.Worker.Services;

namespace OPCAIC.Worker
{
	/// <summary>
	///   The main class of the opcaic worker process
	/// </summary>
	public class Worker : IHostedService
	{
		private readonly IWorkerConnector connector;
		private readonly ExecutionConfig executionConfig;
		private readonly ILogger logger;

		private Thread SocketThread;
		private Thread ConsumerThread;

		private readonly Random rand = new Random();
		private readonly IServiceProvider serviceProvider;

		private CancellationTokenSource currentTaskCts;

		public Worker(IWorkerConnector connector, ILogger<Worker> logger,
			IServiceProvider serviceProvider, IOptions<ExecutionConfig> executionConfig)
		{
			this.connector = connector;
			this.logger = logger;
			this.serviceProvider = serviceProvider;
			this.executionConfig = executionConfig.Value;

			// setup threads
			SocketThread = new Thread(connector.EnterSocket);
			SocketThread.Name = $"{Identity} - Socket";
			ConsumerThread = new Thread(connector.EnterConsumer);
			ConsumerThread.Name = $"{Identity} - Consumer";

			RegisterHandlers();
		}

		private string Identity => connector.Identity;

		private void RegisterHandlers()
		{
			connector.RegisterAsyncHandler<MatchExecutionRequest>(
				ServeRequest<MatchExecutionRequest, MatchExecutionResult>);
			connector.RegisterAsyncHandler<SubmissionValidationRequest>(
				ServeRequest<SubmissionValidationRequest, SubmissionValidationResult>);
			connector.RegisterHandler<WorkerResetMessage>(Reset);
		}

		private void Reset(WorkerResetMessage msg)
		{
			logger.LogInformation($"{nameof(WorkerResetMessage)} received, resetting heartbeat config.");
			connector.SetHeartbeatConfig(msg.HeartbeatConfig);

			// claim ownership of the CTS
			var cts = Interlocked.Exchange(ref currentTaskCts, null);
			if (cts != null)
			{
				logger.LogInformation("Aborting current task");
				cts.Cancel();
				cts.Dispose();
			}
		}

		private void ServeRequest<TRequest, TResult>(TRequest request)
			where TResult : ReplyMessageBase, new() where TRequest : WorkMessageBase
		{
			TResult res = null;
			Debug.Assert(currentTaskCts == null);

			// keep local (thread-safe) reference
			var cts = new CancellationTokenSource(executionConfig.MaxTaskTimeout);
			currentTaskCts = cts;

			try
			{
				using (var scope = serviceProvider.CreateScope())
				{
					res = scope.ServiceProvider.GetRequiredService<IJobExecutor<TRequest, TResult>>()
						.Execute(request, cts.Token);
				}
			}
			catch (Exception e)
			{
				logger.LogError(e,
					$"Exception occured when processing message of type {typeof(TRequest)}: {JsonConvert.SerializeObject(request)}");
			}

			// Make sure we always send non-null message
			if (res == null)
			{
				res = new TResult
				{
					Id = request.Id,
					Status = Status.Error
				};
			}

			// atomically dispose of the cancellation token source
			Interlocked.Exchange(ref currentTaskCts, null)?.Dispose();

			connector.SendMessage(res);
		}

		public void Run()
		{
			logger.LogInformation("Starting Worker");
			var t = new Thread(connector.EnterSocket);
			t.Start();

			InitConnection();
			connector.EnterConsumer(); // returns on worker exit

			connector.StopSocket();
			t.Join();
		}

		public void InitConnection()
		{
			logger.LogInformation($"[{Identity}] - Initiating connection");

			connector.SendMessage(new WorkerConnectMessage
			{
				Capabilities = new WorkerCapabilities
				{
					SupportedGames = serviceProvider.GetService<IGameModuleRegistry>().GetAllModules()
						.Select(m => m.GameName).ToList()
				}
			});
		}

		/// <inheritdoc />
		public async Task StartAsync(CancellationToken cancellationToken)
		{
			logger.LogInformation("Starting Worker");
			SocketThread.Start();
			ConsumerThread.Start();
		}

		/// <inheritdoc />
		public async Task StopAsync(CancellationToken cancellationToken)
		{
			logger.LogInformation("Shutting down Worker");
			connector.StopSocket();
			connector.StopConsumer();
			SocketThread.Join();
			ConsumerThread.Join();
		}
	}
}
