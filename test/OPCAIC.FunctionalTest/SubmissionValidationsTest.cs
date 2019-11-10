using System.Threading.Tasks;
using OPCAIC.Application.Infrastructure;
using OPCAIC.Application.MatchExecutions.Models;
using OPCAIC.Application.SubmissionValidations.Models;
using OPCAIC.FunctionalTest.Infrastructure;
using Shouldly;
using Xunit;
using Xunit.Abstractions;

namespace OPCAIC.FunctionalTest
{
	public class SubmissionValidationsTest : FunctionalTestBase
	{
		/// <inheritdoc />
		public SubmissionValidationsTest(ITestOutputHelper output, FunctionalTestFixture fixture) : base(output, fixture)
		{
		}

		[Fact]
		public async Task ListAll()
		{
			await LoginAsAdmin();

			var result = await GetAsync<PagedResult<SubmissionValidationPreviewDto>>("api/validation?count=10");

			result.Total.ShouldBeGreaterThan(0);
		}

		[Fact]
		public async Task Get()
		{
			await LoginAsAdmin();

			var result = await GetAsync<SubmissionValidationDetailDto>("api/validation/5");
		}

		[Fact]
		public async Task GetAdmin()
		{
			await LoginAsAdmin();

			var result = await GetAsync<SubmissionValidationAdminDto>("api/validation/5/admin");
		}

		[Fact]
		public async Task Download()
		{
			await LoginAsAdmin();

			var response = await GetAsync("api/validation/5/result", false);
			response.EnsureSuccessStatusCode();
		}
	}
}