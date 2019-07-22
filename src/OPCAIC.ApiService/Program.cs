using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using OPCAIC.ApiService.Utils;
using OPCAIC.Infrastructure.DbContexts;

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
				.UseStartup<Startup>();
	}
}