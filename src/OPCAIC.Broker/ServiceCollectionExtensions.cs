using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using OPCAIC.Messaging;
using OPCAIC.Messaging.Config;

namespace OPCAIC.Broker
{
	public static class ServiceCollectionExtensions
	{
		public static IServiceCollection AddBroker(this IServiceCollection services)
		{
			return services
				.AddSingleton<IBrokerConnector, BrokerConnector>()
				.AddSingleton<IBroker, Broker>()
				// initialize the broker as part of the hosted service infrastructure
				.AddSingleton<IHostedService, Broker>(
					sp => (Broker)sp.GetRequiredService<IBroker>());
		}
	}
}