using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OPCAIC.Messaging;

namespace OPCAIC.Worker
{
	internal class Program
	{
		public static void ConfigureServices(IServiceCollection services, ILoggerFactory loggerFactory,
			IConfiguration configuration)
		{
			loggerFactory.AddLog4Net();

			var config = new WorkerConnectorConfig();
			configuration.Bind("ConnectorConfig", config);
			services
				.AddSingleton(config)
				.AddSingleton<WorkerConnector>()
				.AddSingleton<Worker>();
		}

		public static void Main(string[] args)
		{
			var config = new ConfigurationBuilder()
				.AddJsonFile("appsettings.json")
				.Build();
			var logger = new LoggerFactory();
			var services = new ServiceCollection()
				.AddLogging(builder => builder.Services.AddSingleton<ILoggerFactory>(logger))
				.AddSingleton<Application>();

			ConfigureServices(services, logger, config);

			services.BuildServiceProvider().GetRequiredService<Application>().Run();
		}
	}
}
