using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using OPCAIC.Broker;
using OPCAIC.Infrastructure.Repositories;
using OPCAIC.Messaging.Messages;

namespace OPCAIC.ApiService.Services
{
	public class BrokerReactor : IHostedService
	{
		private readonly IBroker broker;
		private readonly IServiceProvider serviceProvider;

		public BrokerReactor(IBroker broker, IServiceProvider serviceProvider)
		{
			this.broker = broker;
			this.serviceProvider = serviceProvider;

			broker.RegisterHandler<MatchExecutionResult>(msg => Task.Run(() => OnMatchExecuted(msg)));
			broker.RegisterHandler<SubmissionValidationResult>(msg => Task.Run(() => OnSubmissionValidated(msg)));
		}

		private Task OnMatchExecuted(MatchExecutionResult result)
		{
			// TODO
			return Task.CompletedTask;
		}

		private async Task OnSubmissionValidated(SubmissionValidationResult result)
		{
			using (var scope = serviceProvider.CreateScope())
			{
				var validationService = scope.ServiceProvider.GetRequiredService<ISubmissionValidationService>();
				await validationService.UpdateFromMessage(result);
			}
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
	}
}