using System;
using System.Collections.Generic;
using System.Data.Common;
using System.IO;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using OPCAIC.ApiService;
using OPCAIC.ApiService.Utils;
using OPCAIC.Persistence;

namespace OPCAIC.FunctionalTest
{
	public class WebServerFactory : WebServerFactory<Startup>
	{
	}

	public class WebServerFactory<TStartup> : WebApplicationFactory<TStartup> where TStartup : class
	{
		private readonly DbConnection DbConnection = new SqliteConnection("DataSource=:memory:");

		private readonly List<Action<IServiceCollection>> configureAction =
			new List<Action<IServiceCollection>>();

		private readonly DirectoryInfo tempStorageDir =
			Directory.CreateDirectory(Path.Combine(Path.GetTempPath(),
				Guid.NewGuid().ToString()));

		public void AddServiceOverride<TService>(TService impl) where TService : class
		{
			configureAction.Add(sp => sp.AddSingleton(impl));
		}

		/// <inheritdoc />
		protected override TestServer CreateServer(IWebHostBuilder builder)
		{
			builder.UseSolutionRelativeContentRoot("src/OPCAIC.ApiService");

			// overwrite some configuration and services for test purposes
			builder
				.UseEnvironment("Production")
				.ConfigureServices((ctx, services) =>
				{
					ctx.Configuration["ConnectionStrings:DataContext"] = "unused"; // to avoid exception from Npsql
					ctx.Configuration["Storage:Directory"] =
						tempStorageDir.FullName + Path.DirectorySeparatorChar;
				})
				.ConfigureTestServices(services =>
				{
					// sqlite needs a connection to be open in order to keep data in memory
					DbConnection.Open();
					var options = new DbContextOptionsBuilder<DataContext>()
						.UseSqlite(DbConnection)
						.Options;

					services.AddSingleton(options);
					services.AddSingleton<DbContextOptions>(options);

					foreach (var action in configureAction)
					{
						action(services);
					}

					// initialize db
					using (var scope = services.BuildServiceProvider().CreateScope())
					{
						ApplicationTestSeed.Seed(scope.ServiceProvider);
					}
				});


			return base.CreateServer(builder);
		}

		/// <inheritdoc />
		protected override void Dispose(bool disposing)
		{
			base.Dispose(disposing);
			tempStorageDir.Delete(true);
		}
	}
}