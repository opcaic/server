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

			int i = 0;

			configuration.Bind("Heartbeat", heartbeatConfig);
			services
				.AddSingleton(heartbeatConfig)
				.AddLogging(builder => builder.Services.AddSingleton(loggingFactory))
				.AddOptions()
				.AddSingleton(sf => new BrokerConnector(
					Worker.Worker.ConnectionString,
					"Broker",
					sf.GetRequiredService<HeartbeatConfig>()))
				.AddSingleton<Broker>()
				.AddTransient(sf => new WorkerConnector(
					Worker.Worker.ConnectionString,
					$"Worker{i++}",
					sf.GetRequiredService<HeartbeatConfig>()))
				.AddTransient<Worker.Worker>()
				.AddSingleton<Application>();
		}

		public static int Main(string[] args)
		{
			if (args.Length != 2 || !int.TryParse(args[1], out port))
			{
				Console.WriteLine("Usage: [host] [port]");
				return 1;
			}
			Worker.Worker.ConnectionString = $"tcp://localhost:{port}";

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
