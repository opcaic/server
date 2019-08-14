using System.Runtime.CompilerServices;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

[assembly: InternalsVisibleTo("OPCAIC.Worker.Test")]

namespace OPCAIC.Worker
{
	internal class Program
	{
		public static void Main(string[] args)
		{
			var host = CreateHostBuilder(args).Build();
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
				.ConfigureLogging(Startup.ConfigureLogging)
				.ConfigureServices(Startup.ConfigureServices)
				.UseConsoleLifetime();
		}
	}
}