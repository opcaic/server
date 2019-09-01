using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OPCAIC.Broker;
using OPCAIC.Messaging.Messages;
using OPCAIC.Utils;

namespace OPCAIC.ApiService.Services
{
	public class BrokerReactor : IHostedService
	{
		private readonly IServiceProvider serviceProvider;
		private readonly ILogger<BrokerReactor> logger;

		public BrokerReactor(IBroker broker, IServiceProvider serviceProvider, ILogger<BrokerReactor> logger)
		{
			this.serviceProvider = serviceProvider;
			this.logger = logger;

			broker.RegisterHandler<MatchExecutionResult>(
				msg => Task.Run(() => ScopeExecute(sp => OnMatchExecuted(sp, msg))));
			broker.RegisterHandler<SubmissionValidationResult>(
				msg => Task.Run(() => ScopeExecute(sp => OnSubmissionValidated(sp, msg))));
		}


		/// <inheritdoc />
		public Task StartAsync(CancellationToken cancellationToken)
		{
			return Task.CompletedTask;
		}

		/// <inheritdoc />
		public Task StopAsync(CancellationToken cancellationToken)
		{
			return Task.CompletedTask;
		}

		private async Task ScopeExecute(Func<IServiceProvider, Task> action)
		{
			var scope = serviceProvider.CreateScope();
			try
			{
				await action(scope.ServiceProvider);
			}
			catch (Exception e) when (Log(e))
			{ 
				// already logged in Log(e)
			}
			finally
			{
				scope.Dispose();
			}
		}

		private bool Log(Exception e)
		{
			logger.LogCritical(e, e.Message);
			return true;
		}

		private async Task OnMatchExecuted(IServiceProvider services, MatchExecutionResult result)
		{
			using (logger.BeginScope((LoggingTags.JobId, result.JobId)))
			{
				var executionService = services.GetRequiredService<IMatchExecutionService>();
				await executionService.UpdateFromMessage(result);
			}
		}

		private async Task OnSubmissionValidated(IServiceProvider services, SubmissionValidationResult result)
		{
			using (logger.BeginScope((LoggingTags.JobId, result.JobId)))
			{
				var validationService = services.GetRequiredService<ISubmissionValidationService>();
				await validationService.UpdateFromMessage(result);
			}
		}
	}
}