using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace OPCAIC.Broker.Runner
{
	internal class Program
	{
		private static int port;
		private static int counter;

		public static bool stop;

		public static int Main(string[] args)
		{
			var config = new ConfigurationBuilder()
				.AddJsonFile("appsettings.json")
				.Build();
			var logger = new LoggerFactory();
			var services = new ServiceCollection()
				.AddLogging(builder => builder.Services.AddSingleton<ILoggerFactory>(logger))
				.AddSingleton<Application>();

			Startup.ConfigureLogging(logger);
			Startup.ConfigureServices(services);
			Startup.Configure(config, services);

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
