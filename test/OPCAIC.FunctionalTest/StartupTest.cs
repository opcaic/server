using System;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace OPCAIC.FunctionalTest
{
	public class StartupTest : FunctionalTestBase
	{
		[Fact]
		public async Task Startup()
		{
			var response = await GetAsync("/api/health");

			response.EnsureSuccessStatusCode();
		}

		/// <inheritdoc />
		public StartupTest(ITestOutputHelper output, FunctionalTestFixture fixture) : base(output, fixture)
		{
		}
	}
}
