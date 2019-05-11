using OPCAIC.Messaging;
using System;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OPCAIC.Worker;

namespace OPCAIC.Broker.Runner
{
	internal class Program
	{
		private static int port;
		private static int counter;

		public static bool stop;

		public static void ConfigureServices(IServiceCollection services, ILoggerFactory logger, IConfiguration config)
		{
			logger.AddLog4Net();

			var heartbeatConfig = new HeartbeatConfig();
			config.Bind("Heartbeat", heartbeatConfig);
			var brokerConfig = new BrokerConnectorConfig();
			config.Bind("Broker", brokerConfig);
			brokerConfig.HeartbeatConfig = heartbeatConfig;
			var workerSetConfig = new WorkerSetConfig();
			config.Bind("WorkerSet", workerSetConfig);
			int i = 0;
			services
				.AddSingleton(heartbeatConfig)
				.AddSingleton(brokerConfig)
				.AddSingleton(workerSetConfig)
				.AddOptions()
				.AddSingleton(sf =>
				{
					var cfg = sf.GetRequiredService<BrokerConnectorConfig>();
					return new BrokerConnector(
						cfg.ListeningAddress,
						cfg.Identity,
						cfg.HeartbeatConfig);
				})
				.AddSingleton<Broker>();
		}

		public static int Main(string[] args)
		{
			var config = new ConfigurationBuilder()
				.AddJsonFile("appsettings.json")
				.Build();
			var logger = new LoggerFactory();
			var services = new ServiceCollection()
				.AddLogging(builder => builder.Services.AddSingleton(logger))
				.AddSingleton<Application>();

			ConfigureServices(services, logger, config);

			Console.CancelKeyPress += (_, a) =>
			{
				a.Cancel = true;
				stop = true;
			};

			services.BuildServiceProvider().GetRequiredService<Application>().Run();
			return 0;
		}
	}
}
