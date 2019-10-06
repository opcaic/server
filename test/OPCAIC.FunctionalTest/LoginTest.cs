using System;
using System.Net;
using System.Security.Policy;
using System.Threading.Tasks;
using System.Web;
using OPCAIC.ApiService.Models;
using OPCAIC.ApiService.Models.Users;
using OPCAIC.ApiService.Security;
using OPCAIC.ApiService.Users.Commands;
using OPCAIC.Application.Dtos.Users;
using OPCAIC.Application.Emails.Templates;
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

		[Fact]
		public async Task RegisterNewUser()
		{
			var email = "newbie@site.com";
			var response = await PostAsync("api/users", new CreateUserCommand
			{
				Email = email,
				LocalizationLanguage = "en",
				Password = "Pa$Sw0rd",
				Organization = "MFF",
				Username = "Raymond",
				WantsEmailNotifications = false
			});

			response.EnsureSuccessStatusCode();

			var verificationUrl = Fixture.EmailService.GetAllEmails(email).ShouldHaveSingleItem()
				.ShouldBeOfType<EmailType.UserVerificationType.Email>().VerificationUrl;

			verificationUrl.ShouldNotBeNull();

			// extract token from url
			var token = HttpUtility.ParseQueryString(new Uri(verificationUrl).Query)["token"];
			token.ShouldNotBeNull();

			response = await PostAsync("api/users/emailVerification",
				new EmailVerificationModel {Email = email, Token = token});
			response.EnsureSuccessStatusCode();

			// at last, try logging in
			var identity = await PostAsync<UserIdentityModel>("api/users/login", new UserCredentialsModel
			{
				Email = email, Password = "Pa$Sw0rd"
			});
		}
	}
}