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

		public static void ConfigureServices(IServiceCollection services, IConfiguration configuration)
		{
			var loggingFactory = new LoggerFactory()
				.AddLog4Net();

			var heartbeatConfig = new HeartbeatConfig();
			configuration.Bind("Heartbeat", heartbeatConfig);
			var brokerConfig = new BrokerConnectorConfig();
			configuration.Bind("Broker", brokerConfig);
			brokerConfig.HeartbeatConfig = heartbeatConfig;
			var workerSetConfig = new WorkerSetConfig();
			configuration.Bind("WorkerSet", workerSetConfig);
			int i = 0;
			services
				.AddSingleton(heartbeatConfig)
				.AddSingleton(brokerConfig)
				.AddSingleton(workerSetConfig)
				.AddLogging(builder => builder.Services.AddSingleton(loggingFactory))
				.AddOptions()
				.AddSingleton(sf =>
				{
					var config = sf.GetRequiredService<BrokerConnectorConfig>();
					return new BrokerConnector(
						config.ListeningAddress,
						config.Identity,
						config.HeartbeatConfig);
				})
				.AddSingleton<Broker>()
				.AddSingleton<Application>();
		}

		public static int Main(string[] args)
		{
			var config = new ConfigurationBuilder()
				.AddJsonFile("appsettings.json").Build();

			var serviceCollection = new ServiceCollection();
			ConfigureServices(serviceCollection, config);
			var serviceProvider = serviceCollection.BuildServiceProvider();

			Console.CancelKeyPress += (_, a) =>
			{
				a.Cancel = true;
				stop = true;
			};

			serviceProvider.GetRequiredService<Application>().Run();
			return 0;
		}
	}
}
