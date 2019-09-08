using System;
using System.Threading;
using System.Threading.Tasks;
using log4net;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OPCAIC.Infrastructure.Emails;

namespace OPCAIC.ApiService.Services
{
	public class EmailCronService : HostedJob
	{
		private const int tickMilliseconds = 5000;

		public EmailCronService(IServiceProvider serviceProvider, ILogger<EmailCronService> logger) 
			: base(serviceProvider, logger, TimeSpan.FromMilliseconds(tickMilliseconds))
		{
		}

		protected override async Task ExecuteJob(IServiceProvider scopedProvider,
			CancellationToken cancellationToken)
		{
			var sender = scopedProvider.GetRequiredService<EmailSender>();
			await sender.TickAsync(cancellationToken);
		}
	}
}