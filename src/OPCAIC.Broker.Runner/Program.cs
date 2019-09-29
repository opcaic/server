using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Serilog;

namespace OPCAIC.Broker.Runner
{
	internal static class Program
	{
		public static void Main(string[] args)
		{
			CreateHostBuilder(args).Build().Run();
		}

		public static IHostBuilder CreateHostBuilder(string[] args)
		{
			return new HostBuilder().ConfigureAppConfiguration((host, config) =>
				{
					var env = host.HostingEnvironment;

					config
						.AddJsonFile("appsettings.json", true, true)
						.AddJsonFile($"appsettings.{env}.json", true, true)
						.AddEnvironmentVariables()
						.AddCommandLine(args);
				})
				.UseSerilog()
				.ConfigureLogging((context, builder) =>
				{
					Log.Logger = new LoggerConfiguration()
						.ReadFrom.Configuration(context.Configuration)
						.CreateLogger();
				})
				.ConfigureServices(Startup.ConfigureServices)
				.UseConsoleLifetime();
		}
	}
}