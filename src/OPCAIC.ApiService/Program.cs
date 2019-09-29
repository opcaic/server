using System;
using System.Reflection;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OPCAIC.ApiService.Utils;
using OPCAIC.Common;
using OPCAIC.Persistence;
using OPCAIC.Utils;
using Serilog;
using Serilog.Events;
using Serilog.Sinks.Graylog;
using Serilog.Sinks.Graylog.Core.Transport;

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
				//3. Get the instance of BoardGamesDBContext in our services layer
				var services = scope.ServiceProvider;
				var context = services.GetRequiredService<DataContext>();

				//4. Call the DataGenerator to create sample data
				DataGenerator.Initialize(services);
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