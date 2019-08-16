using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OPCAIC.Infrastructure.Emails;

namespace OPCAIC.ApiService.Services
{
	public class EmailCronService : IHostedService
	{
		private const int tickMilliseconds = 5000;
		private readonly CancellationTokenSource cancellationTokenSource;
		private readonly ILogger<EmailCronService> logger;
		private readonly IServiceProvider serviceProvider;

		public EmailCronService(IServiceProvider serviceProvider)
		{
			cancellationTokenSource = new CancellationTokenSource();
			this.serviceProvider = serviceProvider;
			logger = serviceProvider.GetRequiredService<ILogger<EmailCronService>>();
		}

		public Task StartAsync(CancellationToken cancellationToken)
		{
			logger.LogInformation($"{nameof(EmailCronService)} - started.");
			ExecuteAsync(cancellationTokenSource.Token);
			return Task.CompletedTask;
		}

		public Task StopAsync(CancellationToken cancellationToken)
		{
			logger.LogInformation($"{nameof(EmailCronService)} - stopped.");

			cancellationTokenSource.Cancel();
			return Task.CompletedTask;
		}

		private async Task ExecuteAsync(CancellationToken cancellationToken)
		{
			while (!cancellationToken.IsCancellationRequested)
			{
				await Task.Delay(tickMilliseconds);

				try
				{
					using (var scope = serviceProvider.CreateScope())
					{
						var sender = scope.ServiceProvider.GetRequiredService<EmailSender>();
						await sender.TickAsync(cancellationToken);
					}
				}
				catch (Exception ex)
				{
					logger.LogError(ex,
						$"An error has occured during execution of {nameof(EmailCronService)}.");
				}
			}
		}
	}
}