using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace OPCAIC.Broker.Runner
{
	internal static class Program
	{
		public static void Main(string[] args) => CreateHostBuilder(args).Build().Run();

		public static IHostBuilder CreateHostBuilder(string[] args)
			=> new HostBuilder().ConfigureAppConfiguration((host, config) =>
				{
					var env = host.HostingEnvironment;

					config
						.AddJsonFile("appsettings.json", true, true)
						.AddJsonFile($"appsettings.{env}.json", true, true)
						.AddEnvironmentVariables()
						.AddCommandLine(args);
				})
				.ConfigureLogging(Startup.ConfigureLogging)
				.ConfigureServices(Startup.ConfigureServices)
				.UseConsoleLifetime();
	}
}
