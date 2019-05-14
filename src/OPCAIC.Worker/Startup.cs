using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OPCAIC.Messaging;
using OPCAIC.Messaging.Config;
using OPCAIC.Worker.GameModules;

namespace OPCAIC.Worker
{
	public class Startup
	{
		public static void ConfigureLogging(ILoggerFactory loggerFactory)
		{
			loggerFactory.AddLog4Net();
		}

		public static void Configure(IConfiguration configuration, IServiceCollection services)
		{
			var config = new WorkerConnectorConfig();
			configuration.Bind("ConnectorConfig", config);
			services.AddSingleton(config);
		}

		public static void ConfigureServices(IServiceCollection services)
		{
			services
				.AddSingleton<IGameModuleRegistry>(new DummyModuleRegistry(new[]
				{
					"game0",
					"game1",
					"game2",
					"game3",
					"game4",
					"game5"
				}))
				.AddSingleton<WorkerConnector>()
				.AddSingleton<Worker>();
		}
	}
}