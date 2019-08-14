using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OPCAIC.Infrastructure.Emails;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace OPCAIC.ApiService.Services
{
	public class EmailCronService : IHostedService
	{
		private readonly CancellationTokenSource cancellationTokenSource;
		private readonly IServiceProvider serviceProvider;
		private readonly ILogger<EmailCronService> logger;

		private const int tickMilliseconds = 5000;

		public EmailCronService(IServiceProvider serviceProvider)
		{
			this.cancellationTokenSource = new CancellationTokenSource();
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
				catch(Exception ex)
				{
					logger.LogError(ex, $"An error has occured during execution of {nameof(EmailCronService)}.");
				}
			}
		}
	}
}
