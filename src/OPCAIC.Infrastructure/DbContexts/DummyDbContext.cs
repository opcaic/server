namespace OPCAIC.Infrastructure.DbContexts
{
	using Entities;
	using Microsoft.EntityFrameworkCore;

	public class DummyDbContext : DbContext
	{
		public DummyDbContext(DbContextOptions<DummyDbContext> options)
			: base(options)
		{
		}

		public DbSet<Tournament> Tournaments { get; set; }
	}
}
