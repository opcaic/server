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
			long id = 0;
			Output.WriteLine("Creating a new match");
			WithScoped<EntityFrameworkDbContext>(ctx =>
			{
				ctx.Matches.Add(new Match());
				ctx.SaveChanges();
			});

			Output.WriteLine("Deleting the match");
			WithScoped<EntityFrameworkDbContext>(ctx =>
			{
				var match = ctx.Matches.Single();
				id = match.Id;
				Assert.False(match.IsDeleted);
				ctx.Remove(match);
				ctx.SaveChanges();
				Assert.True(match.IsDeleted);
			});
			Assert.NotEqual(0, id);

			Output.WriteLine("Checking the match is soft-deleted");
			WithScoped<EntityFrameworkDbContext>(ctx =>
			{
				var match = ctx.Matches.IgnoreQueryFilters().Single();
				Assert.Equal(id, match.Id);
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
