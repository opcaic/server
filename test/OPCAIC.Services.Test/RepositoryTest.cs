using System;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OPCAIC.ApiService.Test;
using OPCAIC.Domain.Entities;
using OPCAIC.Persistence;
using OPCAIC.TestUtils;
using Shouldly;
using Xunit;
using Xunit.Abstractions;

namespace OPCAIC.Services.Test
{
	public class RepositoryTest : ServiceTestBase
	{
		/// <inheritdoc />
		public RepositoryTest(ITestOutputHelper output) : base(output)
		{
			UseDatabase();
		}

		[Fact]
		public void SoftDelete()
		{
			var faker = new EntityFaker();

			long gameId = 0;
			long tournamentId = 0;
			Output.WriteLine("Creating a game and a tournament");
			WithScoped<DataContext>(ctx =>
			{
				var game = faker.Entity<Game>();
				ctx.Games.Add(game);

				var tournament = faker.Entity<Tournament>();
				tournament.Game = game;
				ctx.Tournaments.Add(tournament);

				ctx.SaveChanges();

				gameId = game.Id;
				tournamentId = tournament.Id;
			});

			Output.WriteLine("Deleting the tournament");
			WithScoped<DataContext>(ctx =>
			{
				var tournament = ctx.Tournaments.Single();
				tournament.IsDeleted.ShouldBeFalse();

				ctx.Remove(tournament);
				ctx.SaveChanges();

				tournament.IsDeleted.ShouldBeTrue();
			});
			Assert.NotEqual(0, tournamentId);

			Output.WriteLine("Checking that the tournament is soft-deleted");
			WithScoped<DataContext>(ctx =>
			{
				ctx.Tournaments.ShouldBeEmpty();

				var tournament = ctx.Tournaments.IgnoreQueryFilters().Single();

				tournament.Id.ShouldBe(tournamentId);
				tournament.GameId.ShouldBe(gameId);
				tournament.IsDeleted.ShouldBeTrue();
			});
		}
	}
}