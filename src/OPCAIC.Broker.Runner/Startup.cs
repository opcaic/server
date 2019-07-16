using Chimera.Extensions.Logging.Log4Net;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OPCAIC.Messaging;
using OPCAIC.Messaging.Config;
using OPCAIC.Worker;

namespace OPCAIC.Broker.Runner
{
	/// <summary>
	///   Configuration class for the hosted console application.
	/// </summary>
	public static class Startup
	{
		/// <summary>
		///   Configures the logging facilities.
		/// </summary>
		/// <param name="host">Host configuration.</param>
		/// <param name="logging">Logging builder to configure.</param>
		public static void ConfigureLogging(HostBuilderContext host, ILoggingBuilder logging)
		{
			logging.AddLog4NetLogging();
		}

		/// <summary>
		///   Configures logging builder to use Log4Net logging.
		/// </summary>
		/// <param name="logging"></param>
		/// <returns></returns>
		private static ILoggingBuilder AddLog4NetLogging(this ILoggingBuilder logging)
		{
			var container = new Log4NetContainer(new Log4NetSettings {ConfigFilePath = "log4net.config", Watch = true});
			container.Initialize();
			return logging.AddProvider(new Log4NetProvider(container));
		}

		/// <summary>
		///   Configures the application services.
		/// </summary>
		/// <param name="host">Host configuration.</param>
		/// <param name="services">Services collection to configure.</param>
		public static void ConfigureServices(HostBuilderContext host, IServiceCollection services)
		{
			var config = host.Configuration;

			services.AddOptions()
				.Configure<AppConfig>(config);

			services.AddBroker(broker => config.Bind("Broker", broker));

			services.AddHostedService<Application>();
		}

	}
}