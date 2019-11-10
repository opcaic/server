using System.Security.Permissions;
using System.Threading.Tasks;
using OPCAIC.Application.Infrastructure;
using OPCAIC.Application.Matches.Models;
using OPCAIC.Domain.Entities;
using OPCAIC.FunctionalTest.Infrastructure;
using Shouldly;
using Xunit;
using Xunit.Abstractions;

namespace OPCAIC.FunctionalTest
{
	public class MatchesTest : FunctionalTestBase
	{
		/// <inheritdoc />
		public MatchesTest(ITestOutputHelper output, FunctionalTestFixture fixture) : base(output, fixture)
		{
		}

		[Fact]
		public async Task ListAll()
		{
			await LoginAsAdmin();

			var result = await GetAsync<PagedResult<MatchPreviewDto>>("api/matches?count=10");

			result.Total.ShouldBeGreaterThan(0);
		}

		[Fact]
		public async Task Get()
		{
			await LoginAsAdmin();

			var result = await GetAsync<MatchDetailDto>("api/matches/5");
		}

		[Fact]
		public async Task GetAdmin()
		{
			await LoginAsAdmin();

			var result = await GetAsync<MatchAdminDto>("api/matches/5/admin");
		}

		[Fact]
		public async Task Execute()
		{
			await LoginAsAdmin();

			var response = await PostAsync("api/matches/5/execute");
			response.EnsureSuccessStatusCode();

			// make sure it works even when called twice
			response = await PostAsync("api/matches/5/execute");
			response.EnsureSuccessStatusCode();

			var adminDetail = await GetAsync<MatchAdminDto>("api/matches/5/admin");
			adminDetail.Executions.Count.ShouldBeGreaterThanOrEqualTo(2);

			var detail = await GetAsync<MatchDetailDto>("api/matches/5");
			detail.LastExecution.Id.ShouldBe(adminDetail.Executions[^1].Id);
		}
	}
}