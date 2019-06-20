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
	public class Worker
	{
		private readonly IWorkerConnector connector;
		private readonly ILogger logger;
		private readonly IServiceProvider serviceProvider;

		private readonly Random rand = new Random();

		public Worker(IWorkerConnector connector, ILogger<Worker> logger,
			IServiceProvider serviceProvider)
		{
			this.connector = connector;
			this.logger = logger;
			this.serviceProvider = serviceProvider;
		}

		private string Identity => connector.Identity;

		private void RegisterHandlers()
		{
			connector.RegisterAsyncHandler<MatchExecutionRequest>(
				ServeRequest<MatchExecutionRequest, MatchExecutionResult>);
			connector.RegisterAsyncHandler<SubmissionValidationRequest>(
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
			logger.LogInformation("Starting Worker");
			RegisterHandlers();

			var t = new Thread(connector.EnterPoller);
			t.Start();

			InitConnection();
			connector.EnterConsumer(); // returns on worker exit

			connector.StopPoller();
			t.Join();
			logger.LogInformation("Shutting down Worker");
		}

		public void InitConnection()
		{
			logger.LogInformation($"[{Identity}] - Initiating connection");

			connector.SendMessage(new WorkerConnectMessage
			{
				Capabilities = new WorkerCapabilities
				{
					SupportedGames = serviceProvider.GetService<IGameModuleRegistry>().GetAllModules().Select(m => m.GameName).ToList()
				}
			});
		}
	}
}
