using System;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using OPCAIC.ApiService.Extensions;
using OPCAIC.ApiService.Models.Users;
using OPCAIC.ApiService.Security;
using OPCAIC.ApiService.Services;
using OPCAIC.ApiService.Users.Commands;
using OPCAIC.Application.Emails.Templates;
using OPCAIC.Application.Users.Model;
using OPCAIC.Domain.Entities;
using OPCAIC.Domain.Enumerations;
using OPCAIC.Domain.Enums;
using OPCAIC.Persistence;
using Shouldly;
using Xunit;
using Xunit.Abstractions;

namespace OPCAIC.FunctionalTest
{
	public class UsersTest : FunctionalTestBase<UsersTest.Setup>
	{
		private static string Password = "Password##@afawef88";

		private User User => FixtureSetup.User;

		public class Setup
		{
			public readonly User User = new User
			{
				Email = "anonymous@user.com",
				LocalizationLanguage = LocalizationLanguage.EN,
				EmailConfirmed = true,
				UserName = "an0nym0u5",
			};

			public Setup(FunctionalTestFixture fixture)
			{
				var mgr = fixture.GetServerService<UserManager>();

				mgr.CreateAsync(User, Password).GetAwaiter().GetResult().ThrowIfFailed();
			}
		}

		[Fact]
		public async Task UpdateUser_Success()
		{
			await LoginAs(User.Email, Password);

			var update = await PutAsync($"api/users/{User.Id}",
				new UpdateUserCommand
				{
					Id = User.Id + 2, // should be ignored by server
					LocalizationLanguage = LocalizationLanguage.CZ,
					Role = User.Role,
					WantsEmailNotifications = false,
				});
			update.EnsureSuccessStatusCode();

			var afterChange = await GetAsync<UserPreviewDto>($"api/users/{User.Id}");

			afterChange.Id.ShouldBe(User.Id);
			afterChange.Email.ShouldBe(User.Email);
			afterChange.EmailVerified.ShouldBeTrue();
			afterChange.UserRole.ShouldBe(User.Role);
		}

		[Fact]
		public async Task UpdateUser_CannotChangeRole()
		{
			await LoginAs(User.Email, Password);

			var update = await PutAsync($"api/users/{User.Id}",
				new UpdateUserCommand
				{
					Id = User.Id + 2, // should be ignored by server
					LocalizationLanguage = LocalizationLanguage.CZ,
					Role = UserRole.Admin,
					WantsEmailNotifications = false,
					RequestingUserRole = UserRole.Admin // should be ignored
				});

			update.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
		}

		[Fact]
		public async Task RegisterNewUser()
		{
			var email = "newbie@site.com";
			var response = await PostAsync("api/users", new CreateUserCommand
			{
				Email = email,
				LocalizationLanguage = LocalizationLanguage.EN,
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

		/// <inheritdoc />
		public UsersTest(ITestOutputHelper output, FunctionalTestFixture fixture, Setup fixtureSetup) : base(output, fixture, fixtureSetup)
		{
		}
	}
}