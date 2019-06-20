using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using OPCAIC.TestUtils;
using Xunit.Abstractions;

namespace OPCAIC.Worker.Test
{
	public class WorkerTestBase
	{
		protected readonly MockingServiceCollection Services;

		protected ITestOutputHelper Output;

		public WorkerTestBase(ITestOutputHelper output)
		{
			Output = output;

			Services = new MockingServiceCollection();
			Services
				.AddXUnitLogging(output);
		}

		protected T GetService<T>() where T : class
		{
			Services.TryAddTransient<T>();
			return Services.BuildServiceProvider().GetRequiredService<T>();
		}
	}
}
