using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
[assembly: System.Runtime.CompilerServices.InternalsVisibleTo("OPCAIC.Worker.Test")]

namespace OPCAIC.Worker
{
	internal class Program
	{
		public static void Main(string[] args)
		{
			var config = new ConfigurationBuilder()
				.AddJsonFile("appsettings.json", false, true)
				.Build();
			var logger = new LoggerFactory();
			var services = new ServiceCollection()
				.AddLogging(builder => builder.Services.AddSingleton<ILoggerFactory>(logger))
				.AddSingleton<Application>();

			Startup.ConfigureLogging(logger);
			Startup.ConfigureServices(services);
			Startup.Configure(config, services);

			services.BuildServiceProvider().GetRequiredService<Application>().Run();
		}
	}
}
