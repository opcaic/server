using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OPCAIC.Messaging;
using OPCAIC.Worker;

namespace OPCAIC.Broker.Runner
{
	internal class Program
	{
		private static int port;
		private static int counter;

		public static bool stop;

		public static void ConfigureServices(IServiceCollection services, ILoggerFactory loggerFactory,
			IConfiguration configuration)
		{
			loggerFactory.AddLog4Net();

			var heartbeatConfig = new HeartbeatConfig();
			configuration.Bind("Heartbeat", heartbeatConfig);
			var brokerConfig = new BrokerConnectorConfig();
			configuration.Bind("Broker", brokerConfig);
			brokerConfig.HeartbeatConfig = heartbeatConfig;
			var workerSetConfig = new WorkerSetConfig();
			configuration.Bind("WorkerSet", workerSetConfig);
			var i = 0;
			services
				.AddSingleton(heartbeatConfig)
				.AddSingleton(brokerConfig)
				.AddSingleton(workerSetConfig)
				.AddTransient<BrokerConnector>()
				.AddSingleton<Broker>();
		}

		public static int Main(string[] args)
		{
			var config = new ConfigurationBuilder()
				.AddJsonFile("appsettings.json")
				.Build();
			var logger = new LoggerFactory();
			var services = new ServiceCollection()
				.AddLogging(builder => builder.Services.AddSingleton<ILoggerFactory>(logger))
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
