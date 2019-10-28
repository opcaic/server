using System.Threading.Tasks;
using OPCAIC.ApiService.Models;
using OPCAIC.Application.Documents.Commands;
using OPCAIC.Application.Documents.Models;
using OPCAIC.Domain.Entities;
using OPCAIC.Domain.Enums;
using OPCAIC.FunctionalTest.Infrastructure;
using Shouldly;
using Xunit;
using Xunit.Abstractions;

namespace OPCAIC.FunctionalTest
{
	public class DocumentTest : FunctionalTestBase<DocumentTest.Setup>
	{
		public class Setup
		{
			public readonly Game Game;
			public readonly User Owner;
			public readonly string Password;
			public readonly Tournament Tournament;

			public Setup(FunctionalTestFixture fixture)
			{
				Game = fixture.CreateTwoPlayerGame();
				(Owner, Password) = fixture.CreateUser(UserRole.Organizer);
				Tournament = fixture.CreateTournament(Owner, Game);
			}
		}

		/// <inheritdoc />
		public DocumentTest(ITestOutputHelper output, FunctionalTestFixture fixture, Setup fixtureSetup) : base(output, fixture, fixtureSetup)
		{
		}

		[Fact]
		public async Task CreateNewDocument()
		{
			await LoginAs(FixtureSetup.Owner.Email, FixtureSetup.Password);

			var model = new CreateDocumentCommand
			{
				Name = "My document",
				Content = "The document content",
				TournamentId = FixtureSetup.Tournament.Id
			};
			var id = await PostAsync<IdModel>("/api/documents", model);

			var returned = await GetAsync<DocumentDto>($"/api/documents/{id.Id}");
			returned.Id.ShouldBe(id.Id);
			returned.Name.ShouldBe(model.Name);
			returned.Content.ShouldBe(model.Content);
			returned.Tournament.Id.ShouldBe(model.TournamentId);
			returned.Tournament.Name.ShouldBe(FixtureSetup.Tournament.Name);
		}

		[Fact]
		public async Task UpdateDocument()
		{
			await LoginAs(FixtureSetup.Owner.Email, FixtureSetup.Password);

			var model = new CreateDocumentCommand
			{
				Name = "My document",
				Content = "The document content",
				TournamentId = FixtureSetup.Tournament.Id
			};
			var id = await PostAsync<IdModel>("/api/documents", model);

			var updateDto = new UpdateDocumentCommand
			{
				Name = "My new document", Content = "My new content"
			};

			var response = await PutAsync($"/api/documents/{id.Id}", updateDto);
			response.EnsureSuccessStatusCode();
		}

		[Fact]
		public async Task DeleteDocument()
		{
			await LoginAs(FixtureSetup.Owner.Email, FixtureSetup.Password);

			var model = new CreateDocumentCommand
			{
				Name = "My document",
				Content = "The document content",
				TournamentId = FixtureSetup.Tournament.Id
			};
			var id = await PostAsync<IdModel>("/api/documents", model);

			var response = await DeleteAsync( $"/api/documents/{id.Id}");
			response.EnsureSuccessStatusCode();
		}
	}
}