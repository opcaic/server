using System;
using System.Net;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Linq;
using OPCAIC.ApiService.Models;
using OPCAIC.Application.Tournaments.Commands;
using OPCAIC.Application.Tournaments.Models;
using OPCAIC.Domain.Entities;
using OPCAIC.Domain.Enums;
using OPCAIC.Persistence;
using Shouldly;
using Xunit;
using Xunit.Abstractions;

namespace OPCAIC.FunctionalTest
{
	public class TournamentTest : FunctionalTestBase<TournamentTest.Setup>
	{
		public class Setup
		{
			public readonly Game Game;

			public Setup(FunctionalTestFixture fixture)
			{
				var ctx = fixture.GetServerService<DataContext>();
				Game = new Game
				{
					Name = "A testing game",
					Key = "testGame1231241",
					Type = GameType.TwoPlayer,
					Description = "A trivial game for testing purposes",
					ImageUrl =
						"https://images.chesscomfiles.com/uploads/v1/article/17623.87bb05cd.668x375o.47d81802f1eb@2x.jpeg",
					DefaultTournamentImageOverlay = 0.7f,
					DefaultTournamentImageUrl =
						"https://images.chesscomfiles.com/uploads/v1/article/17623.87bb05cd.668x375o.47d81802f1eb@2x.jpeg",
					DefaultTournamentThemeColor = "#555555",
					MaxAdditionalFilesSize = 1024 * 1024,
					ConfigurationSchema = "{}"
				};

				ctx.Games.Add(Game);
				ctx.SaveChanges();
			}
		}

		[Fact]
		public async Task GetTournament_NotFound()
		{
			var response = await GetAsync("/api/tournaments/123124134315");
			response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
		}

		[Fact]
		public async Task CreateAndEditTournament()
		{
			Log("Create a tournament:");
			var command = new CreateTournamentCommand
			{
				Name = "A testing tournament",
				Configuration = new JObject(), // empty,
				Scope = TournamentScope.Deadline,
				Availability = TournamentAvailability.Public,
				Deadline = DateTime.Now.AddDays(1),
				Format = TournamentFormat.Table,
				MaxSubmissionSize = 1024 * 1024,
				RankingStrategy = TournamentRankingStrategy.Maximum,
				GameId = FixtureSetup.Game.Id
			};

			var id = await PostAsync<IdModel>("/api/tournaments", command);

			Log("Get the created tournament back:");
			var tournament = await GetAsync<TournamentDetailDto>($"/api/tournaments/{id.Id}");

			tournament.Id.ShouldBe(id.Id);
			tournament.Name.ShouldBe(command.Name);
			tournament.Configuration.ShouldBe(command.Configuration);
			tournament.Scope.ShouldBe(command.Scope);
			tournament.Availability.ShouldBe(command.Availability);
			tournament.Deadline.ShouldBe(command.Deadline.Value.ToUniversalTime());
			tournament.Format.ShouldBe(command.Format);
			tournament.MaxSubmissionSize.ShouldBe(command.MaxSubmissionSize);
			tournament.RankingStrategy.ShouldBe(command.RankingStrategy);
			tournament.Game.Id.ShouldBe(command.GameId);

			Log("Update the tournament name");
			var update = new UpdateTournamentCommand
			{
				Name = "A testing tournament 2",
				Availability = tournament.Availability,
				Configuration = tournament.Configuration,
				Description = tournament.Description,
				Deadline = tournament.Deadline,
				Format = tournament.Format,
				ImageUrl = tournament.ImageUrl,
				ImageOverlay = tournament.ImageOverlay,
				MatchesPerDay = tournament.MatchesPerDay,
				MaxSubmissionSize = tournament.MaxSubmissionSize,
				PrivateMatchLog = tournament.PrivateMatchLog,
				MenuItems = tournament.MenuItems,
				RankingStrategy = tournament.RankingStrategy,
				Scope = tournament.Scope,
				ThemeColor = tournament.ThemeColor
			};
			update.Name.ShouldNotBe(tournament.Name);

			var response = await PutAsync($"/api/tournaments/{id.Id}", update);
			response.StatusCode.ShouldBe(HttpStatusCode.OK);

			Log("Check that the name was updated");
			tournament = await GetAsync<TournamentDetailDto>($"/api/tournaments/{id.Id}");
			tournament.Name.ShouldBe(update.Name);
		}

		/// <inheritdoc />
		public TournamentTest(ITestOutputHelper output, FunctionalTestFixture fixture, Setup fixtureSetup) : base(output, fixture, fixtureSetup)
		{
			LoginAsAdmin().GetAwaiter().GetResult();
		}
	}
}