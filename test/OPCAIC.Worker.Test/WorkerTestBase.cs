using System;
using Microsoft.Extensions.DependencyInjection;
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

		protected T GetService<T>() => Services.BuildServiceProvider().GetRequiredService<T>();
	}
}
