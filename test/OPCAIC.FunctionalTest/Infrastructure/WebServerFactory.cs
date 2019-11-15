using System;
using System.Collections.Generic;
using System.Data.Common;
using System.IO;
using System.Linq;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OPCAIC.ApiService;
using OPCAIC.ApiService.Configs;
using OPCAIC.ApiService.Extensions;
using OPCAIC.ApiService.Services;
using OPCAIC.ApiService.Utils;
using OPCAIC.Domain.Enums;
using OPCAIC.Persistence;

namespace OPCAIC.FunctionalTest.Infrastructure
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

		public class TestStartup : Startup
		{
			/// <inheritdoc />
			public override void Configure(IApplicationBuilder app)
			{
				app.UseMiddleware<FakeRemoteAddressMiddleware>();

				base.Configure(app);
			}

			/// <inheritdoc />
			public TestStartup(IConfiguration configuration, IWebHostEnvironment environment, ILogger<Startup> logger) : base(configuration, environment, logger)
			{
			}
		}

		/// <inheritdoc />
		protected override TestServer CreateServer(IWebHostBuilder builder)
		{
			builder.UseSolutionRelativeContentRoot("src/OPCAIC.ApiService");

			// overwrite some configuration and services for test purposes
			builder
				.UseStartup<TestStartup>()
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
					services.AddSingleton<FakeRemoteAddressMiddleware>();

					services.AddTransient<IDatabaseSeed, ApplicationTestSeed>();

					services.Configure<SeedConfig>(cfg =>
					{
						cfg.AdminEmail = "admin@opcaic.com";
						cfg.AdminUsername = "admin";
						cfg.AdminPassword = "Password";
					});

					foreach (var action in configureAction)
					{
						action(services);
					}

					using (var scope = services.BuildServiceProvider().CreateScope())
					{
						// initialize db
						scope.ServiceProvider.GetRequiredService<IDatabaseSeed>().DoSeed();

						// confirm admin email
						var mgr = scope.ServiceProvider.GetRequiredService<UserManager>();
						var user = mgr.Users.Single(u => u.Role == UserRole.Admin);
						var token = mgr.GenerateEmailConfirmationTokenAsync(user).GetAwaiter().GetResult();
						mgr.ConfirmEmailAsync(user, token).GetAwaiter().GetResult().ThrowIfFailed();
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