﻿using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Xunit.Abstractions;

namespace OPCAIC.TestUtils
{
	/// <summary>
	///   Base class for services which require mocking and composition via dependency injection.
	/// </summary>
	public class ServiceTestBase
	{
		protected ITestOutputHelper Output;
		private ServiceProvider provider;
		private MockingServiceCollection services;

		public ServiceTestBase(ITestOutputHelper output)
		{
			Output = output;

			services = new MockingServiceCollection();
			services
				.AddXUnitLogging(output);
		}

		protected MockingServiceCollection Services
			=> services ??
				throw new InvalidOperationException(
					$"Cannot access {nameof(Services)} after the service provider has been built");


		protected IServiceProvider ServiceProvider => provider ?? (provider = BuildServiceProvider());

		private ServiceProvider BuildServiceProvider()
		{
			provider = services.BuildServiceProvider();
			services = null;
			return provider;
		}

		/// <summary>
		///   Gets an instance of service registered for type <see cref="T" />. After this call, it is
		///   no longer possible to access the <see cref="Services" /> Property.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <returns></returns>
		protected T GetService<T>() where T : class
		{
			if (services != null)
			{
				Services.TryAddTransient<T>();
			}

			return ServiceProvider.GetRequiredService<T>();
		}


		/// <summary>
		///   Starts a new thread wrapped in a helper to correctly handle uncaught exceptions.
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
		///   Performs given code block withing new service provider scope and disposes of it afterwards.
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
	}
}
