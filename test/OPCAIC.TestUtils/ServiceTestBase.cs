﻿using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using OPCAIC.Persistence;
using Serilog;
using Xunit.Abstractions;

namespace OPCAIC.TestUtils
{
	/// <summary>
	///     Base class for services which require mocking and composition via dependency injection.
	/// </summary>
	public abstract class ServiceTestBase : IDisposable
	{
		private readonly TestDirectoryManager directoryManager;
		protected readonly ITestOutputHelper Output;
		private ServiceProvider provider;
		private MockingServiceCollection services;

		private readonly IList<IDisposable> toDispose;

		protected ServiceTestBase(ITestOutputHelper output)
		{
			Output = output;
			directoryManager = new TestDirectoryManager();
			toDispose = new List<IDisposable>();

			services = new MockingServiceCollection(output);
			services
				.AddLogging(builder => builder.AddSerilog(new LoggerConfiguration()
					.WriteTo.XunitTestOutput(output).CreateLogger()));
		}

		protected MockingServiceCollection Services
			=> services ??
				throw new InvalidOperationException(
					$"Cannot access {nameof(Services)} after the service provider has been built");


		protected IServiceProvider ServiceProvider
			=> provider ?? (provider = BuildServiceProvider());

		protected Lazy<T> GetLazyService<T>() where T : class
		{
			return new Lazy<T>(GetService<T>);
		}

		/// <inheritdoc />
		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		protected void RegisterDispose(IDisposable item)
		{
			toDispose.Add(item);
		}

		protected void UseDatabase()
		{
			var connection = new SqliteConnection("DataSource=:memory:");
			connection.Open();
			RegisterDispose(connection);
			// random new name so tests can run in parallel
			var dbName = Guid.NewGuid().ToString();
			Services.AddDbContext<DataContext>(options =>
			{
				options.UseSqlite(connection);
//				options.EnableSensitiveDataLogging();
//				options.EnableDetailedErrors();
			});

			Services.BuildServiceProvider()
				.GetRequiredService<DataContext>()
				.Database
				.EnsureCreated();
		}

		private ServiceProvider BuildServiceProvider()
		{
			provider = services.BuildServiceProvider();
			services = null;
			return provider;
		}

		/// <summary>
		///     Gets an instance of service registered for type <see cref="T" />. After this call, it is
		///     no longer possible to access the <see cref="Services" /> Property.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <returns></returns>
		protected T GetService<T>() where T : class
		{
			if (services != null && !typeof(T).IsInterface)
			{
				Services.TryAddTransient<T>();
			}

			return ServiceProvider.GetRequiredService<T>();
		}


		/// <summary>
		///     Starts a new thread wrapped in a helper to correctly handle uncaught exceptions.
		/// </summary>
		/// <param name="action">Main code of the thread.</param>
		/// <param name="description">Optional name of the thread.</param>
		/// <returns></returns>
		protected ThreadHelper StartThread(Action action, string description = "")
		{
			var helper = new ThreadHelper(Output, description);
			helper.Start(action, () => { });
			return helper;
		}

		/// <summary>
		///     Performs given code block withing new service provider scope and disposes of it afterwards.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="call"></param>
		protected void WithScoped<T>(Action<T> call)
		{
			using (var scope = ServiceProvider.CreateScope())
			{
				call(scope.ServiceProvider.GetRequiredService<T>());
			}
		}

		/// <summary>
		///     Creates a new directory, which will be automatically deleted on test teardown.
		/// </summary>
		/// <returns></returns>
		protected DirectoryInfo NewDirectory()
		{
			return directoryManager.GetNewDirectory();
		}

		protected virtual void Dispose(bool disposing)
		{
			if (disposing)
			{
				foreach (var disposable in toDispose)
				{
					disposable.Dispose();
				}
				provider?.Dispose();
				directoryManager.Dispose();
			}
		}
	}
}