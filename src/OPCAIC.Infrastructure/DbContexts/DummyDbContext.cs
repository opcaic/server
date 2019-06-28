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
		public DbSet<Match> Matches { get; set; }
		public DbSet<MatchExecution> MatchExecutions { get; set; }
		public DbSet<Submission> Submissions { get; set; }
		public DbSet<SubmissionMatchResult> SubmissionsMatchResults { get; set; }
	}
}
