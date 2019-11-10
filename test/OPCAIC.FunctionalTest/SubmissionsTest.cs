using System.Threading.Tasks;
using OPCAIC.Application.Dtos.Submissions;
using OPCAIC.Application.Infrastructure;
using OPCAIC.Application.Matches.Models;
using OPCAIC.Application.MatchExecutions.Models;
using OPCAIC.Application.Submissions.Models;
using OPCAIC.FunctionalTest.Infrastructure;
using Shouldly;
using Xunit;
using Xunit.Abstractions;

namespace OPCAIC.FunctionalTest
{
	public class SubmissionsTest : FunctionalTestBase
	{
		/// <inheritdoc />
		public SubmissionsTest(ITestOutputHelper output, FunctionalTestFixture fixture) : base(output, fixture)
		{
		}

		[Fact]
		public async Task ListAll()
		{
			await LoginAsAdmin();

			var result = await GetAsync<PagedResult<SubmissionPreviewDto>>("api/submissions?count=10");

			result.Total.ShouldBeGreaterThan(0);
		}

		[Fact]
		public async Task Get()
		{
			await LoginAsAdmin();

			var result = await GetAsync<MatchExecutionDetailDto>("api/submissions/5");
		}

		[Fact]
		public async Task GetAdmin()
		{
			await LoginAsAdmin();

			var result = await GetAsync<MatchExecutionAdminDto>("api/submissions/5/admin");
		}

		[Fact]
		public async Task Download()
		{
			await LoginAsAdmin();

			var response = await GetAsync("api/submissions/5/download", false);
			response.EnsureSuccessStatusCode();
		}

		[Fact]
		public async Task Validate()
		{
			await LoginAsAdmin();

			var response = await PostAsync("api/submissions/5/validate");
			response.EnsureSuccessStatusCode();

			// make sure it works even when called twice
			response = await PostAsync("api/submissions/5/validate");
			response.EnsureSuccessStatusCode();

			var adminDetail = await GetAsync<SubmissionAdminDto>("api/submissions/5/admin");
			adminDetail.Validations.Count.ShouldBeGreaterThanOrEqualTo(2);

			var detail = await GetAsync<SubmissionDetailDto>("api/submissions/5");
			detail.LastValidation.Id.ShouldBe(adminDetail.Validations[^1].Id);
		}
	}
}