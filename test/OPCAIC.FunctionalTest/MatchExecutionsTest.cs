using System.Threading.Tasks;
using OPCAIC.Application.Infrastructure;
using OPCAIC.Application.Matches.Models;
using OPCAIC.Application.MatchExecutions.Models;
using OPCAIC.FunctionalTest.Infrastructure;
using Shouldly;
using Xunit;
using Xunit.Abstractions;

namespace OPCAIC.FunctionalTest
{
	public class MatchExecutionsTest : FunctionalTestBase
	{
		/// <inheritdoc />
		public MatchExecutionsTest(ITestOutputHelper output, FunctionalTestFixture fixture) : base(output, fixture)
		{
		}

		[Fact]
		public async Task ListAll()
		{
			await LoginAsAdmin();

			var result = await GetAsync<PagedResult<MatchExecutionPreviewDto>>("api/match-execution?count=10");

			result.Total.ShouldBeGreaterThan(0);
		}

		[Fact]
		public async Task Get()
		{
			await LoginAsAdmin();

			var result = await GetAsync<MatchExecutionDetailDto>("api/match-execution/5");
		}

		[Fact]
		public async Task GetAdmin()
		{
			await LoginAsAdmin();

			var result = await GetAsync<MatchExecutionAdminDto>("api/match-execution/5/admin");
		}

		[Fact]
		public async Task Download()
		{
			await LoginAsAdmin();

			var response = await GetAsync("api/match-execution/5/download", false);
			response.EnsureSuccessStatusCode();
		}
	}
}