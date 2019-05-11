using Chimera.Extensions.Logging.Log4Net;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OPCAIC.Messaging;
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
			var heartbeatConfig = new HeartbeatConfig();
			configuration.Bind("Heartbeat", heartbeatConfig);

			var brokerConfig = new BrokerConnectorConfig();
			configuration.Bind("Broker", brokerConfig);
			brokerConfig.HeartbeatConfig = heartbeatConfig;

			var workerSetConfig = new WorkerSetConfig();
			configuration.Bind("WorkerSet", workerSetConfig);

			services
				.AddSingleton(heartbeatConfig)
				.AddSingleton(brokerConfig)
				.AddSingleton(workerSetConfig);
		}

		public static void ConfigureServices(IServiceCollection services)
		{
			services
				.AddTransient<BrokerConnector>()
				.AddSingleton<Broker>();
		}
	}
}