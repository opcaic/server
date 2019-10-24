using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using OPCAIC.ApiService.Utils;
using Serilog;

[assembly: System.Runtime.CompilerServices.InternalsVisibleTo("OPCAIC.ApiService.Test")]

namespace OPCAIC.ApiService
{
	public class Program
	{
		public static void Main(string[] args)
		{
			var host = CreateWebHostBuilder(args).Build();

			using (var scope = host.Services.CreateScope())
			{
				if (!scope.ServiceProvider.GetRequiredService<IDatabaseSeed>().DoSeed())
				{
					return;
				}
			}

			host.Run();
		}

		public static IWebHostBuilder CreateWebHostBuilder(string[] args)
		{
			return WebHost.CreateDefaultBuilder(args)
				.UseSerilog()
				.UseStartup<Startup>()
				.ConfigureLogging((context, builder) =>
				{
					Log.Logger = new LoggerConfiguration()
						.ReadFrom.Configuration(context.Configuration)
						.CreateLogger();
				});
		}
	}
}