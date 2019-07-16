using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using OPCAIC.Messaging;
using OPCAIC.Messaging.Config;
using OPCAIC.Messaging.Messages;
using OPCAIC.Worker.Config;
using OPCAIC.Worker.Services;

namespace OPCAIC.Worker
{
	public static class ServiceCollectionExtensions
	{
		public static IServiceCollection AddWorker(this IServiceCollection services, Action<WorkerConnectorConfig> configuration)
		{
			return services
				.AddSingleton<IWorkerConnector, WorkerConnector>()
				.AddSingleton<Worker>()
				// initialize the broker as part of the hosted service infrastructure
				.AddSingleton<IHostedService, Worker>(sp => sp.GetRequiredService<Worker>())
				.Configure(configuration);
		}
	}
}
