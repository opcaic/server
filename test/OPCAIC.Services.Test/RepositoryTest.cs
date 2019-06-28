using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
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
			long id;
			// create new
			using (var ctx = GetScopedService<EntityFrameworkDbContext>())
			{
				ctx.Matches.Add(new Match());
				ctx.SaveChanges();
			}

			// delete
			using (var ctx = GetScopedService<EntityFrameworkDbContext>())
			{
				var match1 = ctx.Matches.IgnoreQueryFilters().Single();
				var match = ctx.Matches.Single();
				id = match.Id;
				Assert.False(match.IsDeleted);
				ctx.Remove(match);
				ctx.SaveChanges();
				Assert.True(match.IsDeleted);
			}

			// assert
			using (var ctx = GetScopedService<EntityFrameworkDbContext>())
			{
				var match = ctx.Matches.IgnoreQueryFilters().Single();
				Assert.Equal(id, match.Id);
				Assert.True(match.IsDeleted);
			}
		}

		/// <inheritdoc />
		public RepositoryTest(ITestOutputHelper output) : base(output)
		{
			Services.AddDbContext<EntityFrameworkDbContext>(options => options.UseInMemoryDatabase(databaseName: "Dummy"));
		}
	}
}
