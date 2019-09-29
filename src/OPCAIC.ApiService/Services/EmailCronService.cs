using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OPCAIC.Infrastructure.Emails;

namespace OPCAIC.ApiService.Services
{
	public class EmailCronService : HostedJob
	{
		private const int TickMilliseconds = 5000;

		public EmailCronService(IServiceScopeFactory scopeFactory, ILogger<EmailCronService> logger) 
			: base(scopeFactory, logger, TimeSpan.FromMilliseconds(TickMilliseconds))
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