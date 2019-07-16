using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
[assembly: System.Runtime.CompilerServices.InternalsVisibleTo("OPCAIC.Worker.Test")]

namespace OPCAIC.Worker
{
	internal class Program
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
