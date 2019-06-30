using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OPCAIC.Infrastructure.DbContexts;
using OPCAIC.Infrastructure.Entities;
using OPCAIC.TestUtils;
using Xunit;
using Xunit.Abstractions;

namespace OPCAIC.Services.Test
{
	public class RepositoryTest : ServiceTestBase
	{
		[Fact]
		public void SoftDelete()
		{
			long matchId = 0;
			long tournamentId = 0;
			Output.WriteLine("Creating a new match");
			WithScoped<EntityFrameworkDbContext>(ctx =>
			{
				var tournament = new Tournament();
				ctx.Tournaments.Add(tournament);
				ctx.Matches.Add(new Match()
				{
					Tournament = tournament
				});
				ctx.SaveChanges();
			});

			Output.WriteLine("Deleting the match");
			WithScoped<EntityFrameworkDbContext>(ctx =>
			{
				var match = ctx.Matches.Single();
				matchId = match.Id;
				tournamentId = match.TournamentId;
				Assert.False(match.IsDeleted);
				ctx.Remove(match);
				ctx.SaveChanges();
				Assert.True(match.IsDeleted);
			});
			Assert.NotEqual(0, tournamentId);

			Output.WriteLine("Checking that the match is soft-deleted");
			WithScoped<EntityFrameworkDbContext>(ctx =>
			{
				var match = ctx.Matches.IgnoreQueryFilters().Single();
				Assert.Equal(matchId, match.Id);
				Assert.Equal(tournamentId, match.TournamentId);
				Assert.True(match.IsDeleted);
			});
		}

		/// <inheritdoc />
		public RepositoryTest(ITestOutputHelper output) : base(output)
		{
			LoggerFactory factory = new LoggerFactory();
			factory.AddProvider(new XUnitLoggerProvider(output));

			Services.AddDbContext<EntityFrameworkDbContext>(options =>
			{
				options.UseInMemoryDatabase(databaseName: "Dummy");
				options.EnableSensitiveDataLogging();
				options.EnableDetailedErrors();
				options.UseLoggerFactory(factory);
			});
		}
	}
}
