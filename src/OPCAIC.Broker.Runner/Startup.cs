using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace OPCAIC.Broker.Runner
{
	/// <summary>
	///     Configuration class for the hosted console application.
	/// </summary>
	public static class Startup
	{
		/// <summary>
		///     Configures the logging facilities.
		/// </summary>
		/// <param name="host">Host configuration.</param>
		/// <param name="logging">Logging builder to configure.</param>
		public static void ConfigureLogging(HostBuilderContext host, ILoggingBuilder logging)
		{
			logging.AddLog4Net(new Log4NetProviderOptions("log4net.config", true));

			// The ILoggingBuilder minimum level determines the the lowest possible level for
			// logging. The log4net level then sets the level that we actually log at.
			logging.SetMinimumLevel(LogLevel.Debug);
		}

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

			services.AddBroker(broker => config.Bind("Broker", broker));

			services.AddHostedService<Application>();
		}
	}
}