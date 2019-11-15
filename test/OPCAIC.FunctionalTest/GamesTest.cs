using System;
using System.Net;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using OPCAIC.ApiService.Models;
using OPCAIC.Application.Games.Commands;
using OPCAIC.Application.Games.Models;
using OPCAIC.Application.Infrastructure;
using OPCAIC.Application.Tournaments.Commands;
using OPCAIC.Domain.Entities;
using OPCAIC.Domain.Enums;
using OPCAIC.FunctionalTest.Infrastructure;
using Shouldly;
using Xunit;
using Xunit.Abstractions;

namespace OPCAIC.FunctionalTest
{
	public class GamesTest : FunctionalTestBase<GamesTest.Setup>
	{
		public class Setup
		{
			public readonly Game Game;

			public Setup(FunctionalTestFixture fixture)
			{
				Game = fixture.CreateTwoPlayerGame();
			}
		}

		[Fact]
		public async Task CreateNewGame()
		{
			await LoginAsAdmin();

			Log("Create new game");
			var createCommand = new CreateGameCommand
			{
				Key = "superdupergame",
				Name = "Super duper game",
				Description = "eawefawefw",
				MaxAdditionalFilesSize = 1000,
				Type = GameType.SinglePlayer
			};
			var id = (await PostAsync<IdModel>("/api/games/", createCommand)).Id;

			Logout();

			var game = await GetAsync<GameDetailDto>($"/api/games/{id}");

			game.Id.ShouldBe(id);
			game.Key.ShouldBe(createCommand.Key);
			game.Name.ShouldBe(createCommand.Name);
			game.Description.ShouldBe(createCommand.Description);
			game.Type.ShouldBe(createCommand.Type);
		}

		[Fact]
		public async Task ListAllGames()
		{
			var games = await GetAsync<PagedResult<GamePreviewDto>>("/api/games?count=10");

			games.Total.ShouldBeGreaterThan(0);
		}

		[Fact]
		public async Task UpdateGame()
		{
			await LoginAsAdmin();

			var updateCommand = new UpdateGameCommand
			{
				ConfigurationSchema = JObject.Parse(@"{
  'title': 'A registration form',
  'description': 'A simple form example.',
  'type': 'object',
  'required': [
    'firstName',
    'lastName'
  ],
  'properties': {
    'firstName': {
      'type': 'string',
      'title': 'First name',
      'default': 'Chuck'
    },
    'lastName': {
      'type': 'string',
      'title': 'Last name'
    }
  }
}"),
				Id = FixtureSetup.Game.Id,
				Description = FixtureSetup.Game.Description,
				Key = FixtureSetup.Game.Key,
				Type = FixtureSetup.Game.Type,
				Name = FixtureSetup.Game.Name,
				MaxAdditionalFilesSize = 100
			};

			var response = await PutAsync($"/api/games/{updateCommand.Id}", updateCommand);
			response.EnsureSuccessStatusCode();
		}

		[Fact]
		public async Task DeleteGame()
		{
			await LoginAsAdmin();

			Log("Create new game");
			var createCommand = new CreateGameCommand
			{
				Key = "superdupergame3333",
				Name = "Super duper gamewfaew",
				Description = "eawefawefw",
				MaxAdditionalFilesSize = 1000,
				Type = GameType.SinglePlayer
			};
			var id = (await PostAsync<IdModel>("/api/games/", createCommand)).Id;

			Log("Check that it exists");
			await GetAsync<GameDetailDto>($"/api/games/{id}");

			Log("Create a tournament for it");
			var tournamentId = (await PostAsync<IdModel>("/api/tournaments", 
				new CreateTournamentCommand
				{
					GameId = id,
					Availability = TournamentAvailability.Public,
					Configuration = new JObject(),
					Format = TournamentFormat.SinglePlayer,
					Name = "Test tournament",
					RankingStrategy = TournamentRankingStrategy.Maximum,
					MaxSubmissionSize = 1000,
					Scope = TournamentScope.Deadline,
					Deadline = DateTime.Now.AddHours(1),
				})).Id;

			Log("Delete the game");
			(await DeleteAsync($"/api/games/{id}")).EnsureSuccessStatusCode();

			Log("Check that the game is deleted");
			var response = await GetAsync($"/api/games/{id}");
			response.StatusCode.ShouldBe(HttpStatusCode.NotFound);

			Log("Check that the tournament is deleted as well");
			response = await GetAsync($"/api/tournaments/{tournamentId}");
			response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
		}

		/// <inheritdoc />
		public GamesTest(ITestOutputHelper output, FunctionalTestFixture fixture, Setup fixtureSetup) : base(output, fixture, fixtureSetup)
		{
		}
	}
}