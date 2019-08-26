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
		private Task theTask;
		private bool isRunning;

		public EmailCronService(IServiceProvider serviceProvider)
		{
			cancellationTokenSource = new CancellationTokenSource();
			this.serviceProvider = serviceProvider;
			logger = serviceProvider.GetRequiredService<ILogger<EmailCronService>>();
		}

		public Task StartAsync(CancellationToken cancellationToken)
		{
			logger.LogInformation($"{nameof(EmailCronService)} - started.");
			isRunning = true;
			theTask = ExecuteAsync(cancellationTokenSource.Token);
			return Task.CompletedTask;
		}

		public async Task StopAsync(CancellationToken cancellationToken)
		{
			cancellationToken.Register(cancellationTokenSource.Cancel);
			Volatile.Write(ref isRunning, false);
			await theTask;
			logger.LogInformation($"{nameof(EmailCronService)} - stopped.");
		}

		private async Task ExecuteAsync(CancellationToken cancellationToken)
		{
			while (Volatile.Read(ref isRunning))
			{
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

				await Task.Delay(tickMilliseconds, cancellationToken);
			}
		}
	}
}