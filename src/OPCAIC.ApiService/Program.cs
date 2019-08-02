using System;
using System.Linq;
using System.Reflection;
using Gelf.Extensions.Logging;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OPCAIC.ApiService.Utils;
using OPCAIC.Infrastructure.DbContexts;
using OPCAIC.Utils;

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
			=> WebHost.CreateDefaultBuilder(args)
				.UseStartup<Startup>()
				.ConfigureLogging((context, builder) =>
				{
					builder.AddConfiguration(context.Configuration.GetSection("Logging"))
						.AddConsole()
						.AddDebug();

					// use Graylog logging if configured
					if (context.Configuration.GetSection("Logging:GELF").Exists())
					{
						builder.AddGelf(options =>
						{
							// Optional customization applied on top of settings in Logging:GELF configuration section.
							options.LogSource = options.LogSource ?? context.HostingEnvironment.ApplicationName;
							options.AdditionalFields[LoggingTags.MachineName] = Environment.MachineName;
							options.AdditionalFields[LoggingTags.AppVersion] = Assembly
								.GetEntryAssembly()
								.GetCustomAttribute<AssemblyInformationalVersionAttribute>()
								.InformationalVersion;
						});
					}
				});
	}
}