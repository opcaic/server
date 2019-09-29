using System;
using System.Reflection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OPCAIC.Common;
using OPCAIC.Messaging.Config;

namespace OPCAIC.Broker.Runner
{
	/// <summary>
	///     Configuration class for the hosted console application.
	/// </summary>
	public static class Startup
	{
		/// <summary>
		///     Configures the application services.
		/// </summary>
		/// <param name="host">Host configuration.</param>
		/// <param name="services">Services collection to configure.</param>
		public static void ConfigureServices(HostBuilderContext host, IServiceCollection services)
		{
			var config = host.Configuration;

			services.AddOptions()
				.Configure<AppConfig>(config);

			services.AddBroker();
			services.Configure<BrokerConnectorConfig>(config.GetSection("Broker"));

			services.AddHostedService<Application>();
		}
	}
}