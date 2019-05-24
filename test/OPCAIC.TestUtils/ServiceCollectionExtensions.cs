using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OPCAIC.Utils;
using Xunit.Abstractions;

namespace OPCAIC.TestUtils
{
	public static class ServiceCollectionExtensions
	{
		/// <summary>
		///   Enables logging via <see cref="ILogger" /> interfaces into xUnits <see cref="ITestOutputHelper" />.
		/// </summary>
		/// <param name="services">Service collection into which register logging.</param>
		/// <param name="output">Output for the test in question.</param>
		/// <returns></returns>
		public static IServiceCollection AddXUnitLogging(this IServiceCollection services,
			ITestOutputHelper output)
		{
			Require.NotNull(services, nameof(services));
			Require.NotNull(output, nameof(output));

			services.AddSingleton(output);
			services.AddSingleton(typeof(ILogger<>), typeof(XUnitLogger<>));

			return services;
		}
	}
}
