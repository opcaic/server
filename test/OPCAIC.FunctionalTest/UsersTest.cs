using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Mime;
using System.Threading.Tasks;
using System.Web;
using Bogus;
using Newtonsoft.Json.Linq;
using OPCAIC.ApiService.Extensions;
using OPCAIC.ApiService.Models;
using OPCAIC.ApiService.Models.Users;
using OPCAIC.ApiService.Security;
using OPCAIC.ApiService.Services;
using OPCAIC.ApiService.Users.Commands;
using OPCAIC.Application.Dtos.Submissions;
using OPCAIC.Application.Emails.Templates;
using OPCAIC.Application.Infrastructure;
using OPCAIC.Application.TournamentInvitations.Commands;
using OPCAIC.Application.Tournaments.Commands;
using OPCAIC.Application.Tournaments.Models;
using OPCAIC.Application.Tournaments.Queries;
using OPCAIC.Application.Users.Model;
using OPCAIC.Domain.Entities;
using OPCAIC.Domain.Enumerations;
using OPCAIC.Domain.Enums;
using OPCAIC.FunctionalTest.Infrastructure;
using Shouldly;
using Xunit;
using Xunit.Abstractions;

namespace OPCAIC.FunctionalTest
{
	public class UsersTest : FunctionalTestBase<UsersTest.Setup>
	{
		private static readonly string Password = "Password##@afawef88";

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
			afterChange.Role.ShouldBe(User.Role);
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

		private async Task<long> CreateNewUser(string username, string email, string password, string organization = null, bool dump = false)
		{
			var id = await PostAsync<IdModel>("api/users", new CreateUserCommand
			{
				Email = email,
				LocalizationLanguage = LocalizationLanguage.EN,
				Password = password,
				Organization = organization,
				Username = username,
				WantsEmailNotifications = false
			}, dump);

			return id.Id;
		}

		private async Task ValidateUserEmailAddress(string email, bool dump = false)
		{
			var verificationUrl = Fixture.EmailService.GetAllEmails(email)
				.ShouldHaveSingleItem()
				.ShouldBeOfType<EmailType.UserVerificationType.Email>().VerificationUrl;

			verificationUrl.ShouldNotBeNull();

			// extract token from url
			var token = HttpUtility.ParseQueryString(new Uri(verificationUrl).Query)["token"];
			token.ShouldNotBeNull();

			var response = await PostAsync("api/users/emailVerification",
				new EmailVerificationModel {Email = email, Token = token}, dump);
			response.EnsureSuccessStatusCode();
		}

		[Fact]
		public async Task RegisterNewUser()
		{
			var email = Faker.Internet.Email();
			var password = Faker.Internet.Password();
			var username = Faker.Internet.UserName();

			await CreateNewUser(username, email, password, dump: true);
			await ValidateUserEmailAddress(email, dump: true);

			// at last, try logging in
			var identity = await PostAsync<UserIdentityModel>("api/users/login", new UserCredentialsModel
			{
				Email = email, Password = password
			});

			identity.Id.ShouldBePositive();
			identity.Email.ShouldBe(email);
			identity.Role.ShouldBe(UserRole.User);
			identity.AccessToken.ShouldNotBeNullOrWhiteSpace();
			identity.RefreshToken.ShouldNotBeNullOrWhiteSpace();
		}

		[Fact]
		public async Task MultipleRegistrations()
		{
			for (int i = 0; i < 4; i++)
			{
				var response = await PostAsync("api/users", new CreateUserCommand
				{
					Email = Faker.Internet.Email(),
					LocalizationLanguage = LocalizationLanguage.EN,
					Password = "Pa$Sw0rd",
					Organization = "MFF",
					Username = Faker.Internet.UserName(),
					WantsEmailNotifications = false
				});

				if (i < 2)
				{
					response.IsSuccessStatusCode.ShouldBeTrue();
				}
				else
				{
					response.IsSuccessStatusCode.ShouldBeFalse();
				}
			}
		}

		[Fact]
		public async Task DeleteUser()
		{
			// test complex scenario with user heavily used throughout the platform
			var email = Faker.Internet.Email();
			var password = Faker.Internet.Password();
			var username = Faker.Internet.UserName();

			var id = await CreateNewUser(username, email, password);
			await ValidateUserEmailAddress(email);

			await LoginAs(email, password);

			Log("Enroll in a tournament");
			var tournamentList = await GetAsync<PagedResult<TournamentPreviewDto>>("api/tournaments?count=2&acceptsSubmission=true");
			tournamentList.Total.ShouldBePositive();
			tournamentList.List.Count.ShouldBe(2);
			var tournamentId = tournamentList.List[0].Id;
			var tournamentId2 = tournamentList.List[1].Id;
			var submissionId = await PostRandomSubmission(tournamentId);

			Log("Change user role to organizer");
			await LoginAsAdmin();
			await ElevateUser(id, UserRole.Organizer);

			Log("Invite user somewhere");
			await PostAsync($"api/tournaments/{tournamentId2}/participants",
				new InvitePlayersCommand {Emails = new[] {email}});

			Log("Add as manager to some tournament");
			await AddTournamentManager(tournamentId, email);

			await LoginAs(email, password);
			Log("Create your own tournament");
			var newTournament = new CreateTournamentCommand
			{
				Name = "A testing tournament2222",
				Configuration = new JObject(), // empty,
				Scope = TournamentScope.Deadline,
				Availability = TournamentAvailability.Public,
				Deadline = DateTime.Now.AddDays(1),
				Format = TournamentFormat.Table,
				MaxSubmissionSize = 1024 * 1024,
				RankingStrategy = TournamentRankingStrategy.Maximum,
				GameId = 1
			};

			var ownTournamentId = (await PostAsync<IdModel>("/api/tournaments", newTournament)).Id;

			Log("Delete yourself");
			var response = await DeleteAsync($"api/users/{id}");
			response.EnsureSuccessStatusCode();

			await LoginAsAdmin();

			Log("Check that submission still exists");
			var sub = await GetAsync<SubmissionDetailDto>($"api/submissions/{submissionId}");
			sub.Author.ShouldBeNull();

			Log("Check that user is removed from managers of tournament");
			var managers = await GetAsync<string[]>($"api/tournaments/{tournamentId}/managers");
			managers.ShouldNotContain(email);

			Log("Check that the tournament still exists");
			await GetAsync<TournamentDetailDto>($"/api/tournaments/{ownTournamentId}");
		}

		private async Task AddTournamentManager(long tournamentId, string email)
		{
			var response = await PostAsync($"api/tournaments/{tournamentId}/managers/{email}");
			response.EnsureSuccessStatusCode();
			var managers = await GetAsync<string[]>($"api/tournaments/{tournamentId}/managers", false);
			managers.ShouldContain(email);
		}

		private async Task ElevateUser(long id, UserRole role)
		{
			var command = await GetAsync<UpdateUserCommand>($"api/users/{id}", false);
			command.Role = role;
			var response = await PutAsync($"api/users/{id}", command);
			response.EnsureSuccessStatusCode();
		}

		private async Task<long> PostRandomSubmission(long tournamentId)
		{
			await using var ms = new MemoryStream();

			using (var zip = new ZipArchive(ms, ZipArchiveMode.Create, true))
			await using (var entry = new StreamWriter(zip.CreateEntry("file.txt").Open()))
			{
				entry.Write(Faker.Lorem.Paragraph());
			}

			ms.Seek(0, SeekOrigin.Begin);
			var streamContent = new StreamContent(ms);
			streamContent.Headers.ContentType = new MediaTypeHeaderValue(MediaTypeNames.Application.Zip);
			var content = new MultipartFormDataContent
			{
				{new StringContent(tournamentId.ToString()), "TournamentId" },
				{streamContent, "archive", "archive.zip"}
			};

			var response = await PostAsync<IdModel>("api/submissions", content);
			return response.Id;
		}

		/// <inheritdoc />
		public UsersTest(ITestOutputHelper output, FunctionalTestFixture fixture, Setup fixtureSetup) : base(output, fixture, fixtureSetup)
		{
		}
	}
}