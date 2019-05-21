using System;
using System.Linq;
using System.Threading;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using OPCAIC.Messaging;
using OPCAIC.Messaging.Config;
using OPCAIC.Messaging.Messages;
using OPCAIC.Worker.GameModules;
using OPCAIC.Worker.Services;

namespace OPCAIC.Worker
{
	/// <summary>
	///   The main class of the opcaic worker process
	/// </summary>
	public class Worker : IDisposable
	{
		private readonly WorkerConnector connector;
		private readonly ILogger logger;
		private readonly IServiceProvider serviceProvider;

		private readonly Random rand = new Random();

		public Worker(IOptions<WorkerConnectorConfig> config, ILogger<Worker> logger,
			IServiceProvider serviceProvider)
		{
			this.connector = new WorkerConnector(config.Value, logger);
			this.logger = logger;
			this.serviceProvider = serviceProvider;

			RegisterHandlers();
		}

		private string Identity => connector.Identity;

		public void Dispose() => connector?.Dispose();

		private void RegisterHandlers()
		{
			connector.RegisterHandler<MatchExecutionRequest>(
				ServeRequest<MatchExecutionRequest, MatchExecutionResult>);
			connector.RegisterHandler<SubmissionValidationRequest>(
				ServeRequest<SubmissionValidationRequest, SubmissionValidationResult>);
		}

		private void ServeRequest<TRequest, TResult>(TRequest request)
			where TResult : ReplyMessageBase, new() where TRequest : WorkMessageBase
		{
			TResult res = null;

			try
			{
				using (var scope = serviceProvider.CreateScope())
				{
					res = scope.ServiceProvider.GetRequiredService<IJobExecutor<TRequest, TResult>>()
						.Execute(request);
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

			connector.SendMessage(res);
		}

		public void Run()
		{
			var t = new Thread(connector.EnterConsumer);
			t.Start();

			logger.LogInformation($"[{Identity}] - Initiating connection");
			connector.SendMessage(new WorkerConnectMessage
			{
				Capabilities = new WorkerCapabilities
				{
					SupportedGames = serviceProvider.GetService<IGameModuleRegistry>().GetAllModules().Select(m => m.GameName).ToList()
				}
			});
			connector.EnterPoller(); // returns on worker exit
			connector.StopConsumer();
			t.Join();
			logger.LogInformation($"[{Identity}] - client officially dead, restarting in 5s");
		}
	}
}
