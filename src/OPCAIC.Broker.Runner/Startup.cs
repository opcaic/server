using Chimera.Extensions.Logging.Log4Net;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OPCAIC.Messaging;
using OPCAIC.Messaging.Config;
using OPCAIC.Worker;

namespace OPCAIC.Broker.Runner
{
	public class Startup
	{
		public static void ConfigureLogging(ILoggerFactory loggerFactory)
		{
			loggerFactory.AddLog4Net(new Log4NetSettings()
			{
				ConfigFilePath = "log4net.config",
				Watch = true
			});
		}

		public static void Configure(IConfiguration configuration, IServiceCollection services)
		{
			services
				.AddOptions()
				.Configure<AppConfig>(configuration)
				.Configure<BrokerConnectorConfig>(configuration.GetSection("Broker"));
		}

		public static void ConfigureServices(IServiceCollection services)
		{
			services
				.AddSingleton<IBroker, Broker>();
		}
	}
}