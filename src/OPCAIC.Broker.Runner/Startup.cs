using System;
using System.Reflection;
using Gelf.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OPCAIC.Messaging.Config;
using OPCAIC.Utils;

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
			logging.AddConfiguration(host.Configuration.GetSection("Logging"));
			logging.AddLog4Net(new Log4NetProviderOptions("log4net.config", true));

			// use Graylog logging if configured
			if (host.Configuration.GetSection("Logging:GELF").Exists())
			{
				logging.AddGelf(options =>
				{
					// Optional customization applied on top of settings in Logging:GELF configuration section.
					options.LogSource = options.LogSource ??
						host.HostingEnvironment.ApplicationName ?? "OPCAIC.Broker.Runner";
					options.AdditionalFields[LoggingTags.MachineName] = Environment.MachineName;
					options.AdditionalFields[LoggingTags.AppVersion] = Assembly
						.GetEntryAssembly()
						.GetCustomAttribute<AssemblyInformationalVersionAttribute>()
						.InformationalVersion;
				});
			}

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

			services.AddBroker();
			services.Configure<BrokerConnectorConfig>(config.GetSection("Broker"));

			services.AddHostedService<Application>();
		}
	}
}