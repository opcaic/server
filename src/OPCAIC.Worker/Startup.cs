using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OPCAIC.Messaging;
using OPCAIC.Messaging.Config;
using OPCAIC.Worker.GameModules;

namespace OPCAIC.Worker
{
	/// <summary>
	///   Configuration class for the hosted console application.
	/// </summary>
	public static class Startup
	{
		/// <summary>
		///   Configures the logging facilities.
		/// </summary>
		/// <param name="loggerFactory"></param>
		public static void ConfigureLogging(ILoggerFactory loggerFactory) => loggerFactory.AddLog4Net();

		/// <summary>
		///   Configures the configuration facilities.
		/// </summary>
		/// <param name="configuration">Configuration root of the entire application.</param>
		/// <param name="services">Service collection of the application.</param>
		public static void Configure(IConfiguration configuration, IServiceCollection services)
		{
			var config = new WorkerConnectorConfig();
			configuration.Bind("ConnectorConfig", config);
			services.AddSingleton(config);
		}

		/// <summary>
		///   Configures used services for the application.
		/// </summary>
		/// <param name="services">Service collection of the application.</param>
		public static void ConfigureServices(IServiceCollection services)
			=> services
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
