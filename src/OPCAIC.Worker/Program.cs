using System.Runtime.CompilerServices;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Serilog;

[assembly: InternalsVisibleTo("OPCAIC.Worker.Test")]

namespace OPCAIC.Worker
{
	public class Program
	{
		public static void Main(string[] args)
		{
			var host = CreateHostBuilder(args)
				.UseSerilog()
				.ConfigureLogging((context, builder) =>
				{
					Log.Logger = new LoggerConfiguration()
						.ReadFrom.Configuration(context.Configuration)
						.CreateLogger();
				})
				.UseConsoleLifetime()
				.Build();
			host.Run();
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
				.ConfigureServices(Startup.ConfigureServices);
		}
	}
}