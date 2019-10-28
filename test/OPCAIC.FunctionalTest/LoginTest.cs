using System.Net;
using System.Threading.Tasks;
using OPCAIC.ApiService.Models;
using OPCAIC.ApiService.Models.Users;
using OPCAIC.ApiService.Security;
using OPCAIC.Application.Dtos.Users;
using OPCAIC.FunctionalTest.Infrastructure;
using Shouldly;
using Xunit;
using Xunit.Abstractions;

namespace OPCAIC.FunctionalTest
{
	public class LoginTest : FunctionalTestBase
	{
		/// <inheritdoc />
		public LoginTest(ITestOutputHelper output, FunctionalTestFixture fixture) : base(output, fixture)
		{
		}

		[Fact]
		public async Task ReturnsUnauthorized()
		{
			var response = await GetAsync("/api/users");

			response.StatusCode.ShouldBe(HttpStatusCode.Unauthorized);
			var header = response.Headers.WwwAuthenticate.ShouldHaveSingleItem();
			header.Scheme.ShouldBe("Bearer");
		}

		[Fact]
		public async Task SuccessfulLogin()
		{
			// try login as admin
			var identity = await PostAsync<UserIdentityModel>("api/users/login", new UserCredentialsModel
			{
				Email = "admin@opcaic.com", Password = "Password"
			});

			identity.AccessToken.ShouldNotBeNull();
			identity.RefreshToken.ShouldNotBeNull();

			UseAccessToken(identity.AccessToken);

			var user = await GetAsync<UserDetailDto>($"api/users/{identity.Id}");

			// try refresh tokens
			var tokens = await PostAsync<UserTokens>($"api/users/{identity.Id}/refresh",
				new RefreshToken() {Token = identity.RefreshToken});

			tokens.AccessToken.ShouldNotBeNull();
			tokens.RefreshToken.ShouldNotBeNull();
		}
	}
}