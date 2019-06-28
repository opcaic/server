using System;
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
		///   Creates a new scoped service provider and requests instance of service registered for type
		///   <see cref="T" />. The scoped service provider is not explicitly disposed. After this call,
		///   it is no longer possible to access the <see cref="Services" /> Property.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <returns></returns>
		protected T GetScopedService<T>() where T : class
		{
			if (services != null)
			{
				Services.TryAddTransient<T>();
			}

			return ServiceProvider.CreateScope().ServiceProvider.GetRequiredService<T>();
		}
	}
}
