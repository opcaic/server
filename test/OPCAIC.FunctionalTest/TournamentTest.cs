using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using OPCAIC.ApiService.Models;
using OPCAIC.Application.Documents.Commands;
using OPCAIC.Application.Tournaments.Commands;
using OPCAIC.Application.Tournaments.Models;
using OPCAIC.Domain.Entities;
using OPCAIC.Domain.Enums;
using OPCAIC.Domain.ValueObjects;
using OPCAIC.FunctionalTest.Infrastructure;
using OPCAIC.Persistence;
using Shouldly;
using Xunit;
using Xunit.Abstractions;

namespace OPCAIC.FunctionalTest
{
	public class TournamentTest : FunctionalTestBase<TournamentTest.Setup>
	{
		/// <inheritdoc />
		public TournamentTest(ITestOutputHelper output, FunctionalTestFixture fixture,
			Setup fixtureSetup) : base(output, fixture, fixtureSetup)
		{
			LoginAsAdmin().GetAwaiter().GetResult();
		}

		public class Setup
		{
			public readonly Game Game;

			public Setup(FunctionalTestFixture fixture)
			{
				Game = fixture.CreateTwoPlayerGame();
			}
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
			var tournament = await GetAsync<TournamentAdminDto>($"/api/tournaments/{id.Id}/admin");

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
			var update = MakeUpdateCommand(tournament);
			update.Name = "A testing tournament 2";
			update.Name.ShouldNotBe(tournament.Name);

			var response = await PutAsync($"/api/tournaments/{id.Id}", update);
			response.StatusCode.ShouldBe(HttpStatusCode.OK);

			Log("Check that the name was updated");
			var updated = await GetAsync<TournamentDetailDto>($"/api/tournaments/{id.Id}");
			updated.Name.ShouldBe(update.Name);
		}

		private static UpdateTournamentCommand MakeUpdateCommand(TournamentAdminDto tournament)
		{
			return new UpdateTournamentCommand
			{
				Name = tournament.Name,
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
		}

		[Fact]
		public async Task CloneTournament()
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
				GameId = FixtureSetup.Game.Id,
			};

			var id = await PostAsync<IdModel>("/api/tournaments", command);

			Log("Add a document");
			var documentId = await PostAsync<IdModel>("/api/documents",
				new CreateDocumentCommand
				{
					Name = "awefa", Content = "awefawefawefawefawef", TournamentId = id.Id
				});

			Log("Add the document to the tournament");
			var tournament = await GetAsync<TournamentAdminDto>($"/api/tournaments/{id.Id}/admin");
			var update = MakeUpdateCommand(tournament);
			update.MenuItems = new List<MenuItemDto>
			{
				new ExternalUrlMenuItemDto
				{
					Type = MenuItemType.ExternalUrl,
					ExternalLink = "http://www.google.com",
					Text = "TEXT"
				},
				new DocumentLinkMenuItemDto
				{
					Type = MenuItemType.DocumentLink,
					DocumentId = documentId.Id
				}
			};
			var response = await PutAsync($"/api/tournaments/{id.Id}", update);
			response.EnsureSuccessStatusCode();

			Log("Refetch the updated tournament");
			tournament = await GetAsync<TournamentAdminDto>($"/api/tournaments/{id.Id}/admin");

			Log("Try cloning the tournament");
			var cloneId = await PostAsync<IdModel>($"/api/tournaments/{id.Id}/clone");
			var clone = await GetAsync<TournamentAdminDto>($"/api/tournaments/{cloneId.Id}/admin");

			Log("Comparing the two tournaments");
			clone.Id.ShouldNotBe(id.Id);
			clone.Name.ShouldBe(tournament.Name);
			clone.Configuration.ShouldBe(tournament.Configuration);
			clone.Scope.ShouldBe(tournament.Scope);
			clone.Availability.ShouldBe(tournament.Availability);
			clone.Deadline.ShouldBe(tournament.Deadline?.ToUniversalTime());
			clone.Format.ShouldBe(tournament.Format);
			clone.MaxSubmissionSize.ShouldBe(tournament.MaxSubmissionSize);
			clone.RankingStrategy.ShouldBe(tournament.RankingStrategy);
			clone.Game.Id.ShouldBe(tournament.Game.Id);

			// link item should be identical
			clone.MenuItems.Count.ShouldBe(tournament.MenuItems.Count);
			clone.MenuItems[0].ShouldBe(tournament.MenuItems[0]);
			// documents should be unique
			clone.MenuItems[1].Type.ShouldBe(tournament.MenuItems[1].Type);
			var cloneItem1 = clone.MenuItems[1].ShouldBeOfType<DocumentLinkMenuItemDto>();
			var item1 = tournament.MenuItems[1].ShouldBeOfType<DocumentLinkMenuItemDto>();
			cloneItem1.DocumentId.ShouldNotBe(item1.DocumentId);
		}

		[Fact]
		public async Task GetTournament_NotFound()
		{
			var response = await GetAsync("/api/tournaments/123124134315");
			response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
		}
	}
}