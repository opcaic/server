using System.Linq;
using Bogus;
using OPCAIC.ApiService.Extensions;
using OPCAIC.ApiService.Services;
using OPCAIC.ApiService.Test;
using OPCAIC.Domain.Entities;
using OPCAIC.Domain.Enums;
using OPCAIC.Persistence;
using Shouldly;

namespace OPCAIC.FunctionalTest.Infrastructure
{
	public static class SetupHelper
	{
		static readonly EntityFaker EntityFaker = new EntityFaker();
		static readonly Faker Faker = new Faker();

		public static (User user, string password) CreateUser(this FunctionalTestFixture fixture, UserRole role = UserRole.User)
		{
			var mgr = fixture.GetServerService<UserManager>();

			var user = EntityFaker.Entity<User>();
			user.Role = role;
			user.EmailConfirmed = true;

			var password = Faker.Internet.Password();
			var result = mgr.CreateAsync(user, password)
				.GetAwaiter().GetResult();
			result.Succeeded.ShouldBeTrue();

			return (user, password);
		}

		public static Game CreateTwoPlayerGame(this FunctionalTestFixture fixture)
		{
			var ctx = fixture.GetServerService<DataContext>();
			var game = new Game
			{
				Name = "A testing game " + Faker.IndexFaker++,
				Key = "game" + Faker.IndexFaker++,
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

			ctx.Games.Add(game);
			ctx.SaveChanges();
			return game;
		}

		public static Tournament CreateTournament(this FunctionalTestFixture fixture, User owner,
			Game game)
		{
			var ctx = fixture.GetServerService<DataContext>();
			var tournament = EntityFaker.Entity<Tournament>();
			tournament.Game = game;
			tournament.Owner = owner;
			tournament.Format = game.Type.SupportedFormats[0];
			tournament.Scope = TournamentScope.Deadline;

			ctx.Add(tournament);
			ctx.SaveChanges();
			return tournament;
		}

		public static User GetAdminUser(this FunctionalTestFixture fixture)
		{
			var ctx = fixture.GetServerService<DataContext>();
			return ctx.Users.FirstOrDefault(u => u.Role == UserRole.Admin);
		}
	}
}