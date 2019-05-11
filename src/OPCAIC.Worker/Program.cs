using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace OPCAIC.Worker
{
	
	internal class Program
	{
		public static void ConfigureServices(IServiceCollection services, ILoggerFactory logger, IConfiguration config)
		{
		}

		public static void Main(string[] args)
		{
			var config = new ConfigurationBuilder()
				.AddJsonFile("appsettings.json")
				.Build();
			var logger = new LoggerFactory();
			var services = new ServiceCollection()
				.AddLogging(builder => builder.Services.AddSingleton(logger))
				.AddSingleton<Application>();

			ConfigureServices(services, logger, config);

			services.BuildServiceProvider().GetRequiredService<Application>().Run();
		}
	}
}
