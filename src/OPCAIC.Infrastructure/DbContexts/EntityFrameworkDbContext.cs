using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using OPCAIC.Infrastructure.Entities;

namespace OPCAIC.Infrastructure.DbContexts
{
	public class EntityFrameworkDbContext : DbContext
	{
		public EntityFrameworkDbContext(DbContextOptions<EntityFrameworkDbContext> options)
			: base(options)
		{
		}

		public DbSet<Tournament> Tournaments { get; set; }
		public DbSet<Match> Matches { get; set; }
		public DbSet<MatchExecution> MatchExecutions { get; set; }
		public DbSet<Submission> Submissions { get; set; }
		public DbSet<SubmissionMatchResult> SubmissionsMatchResults { get; set; }

		/// <inheritdoc />
		protected override void OnModelCreating(ModelBuilder modelBuilder)
		{
			base.OnModelCreating(modelBuilder);

			RegisterSoftDeleteQueryFilters(modelBuilder);
		}

		private static void RegisterSoftDeleteQueryFilters(ModelBuilder modelBuilder)
		{
			var property = typeof(ISoftDeletable).GetProperty(nameof(ISoftDeletable.IsDeleted));

			foreach (var types in modelBuilder.Model.GetEntityTypes()
				.Where(t => typeof(ISoftDeletable).IsAssignableFrom(t.ClrType)))
			{
				// construct (e => !e.isDeleted) expression
				var parameterExpression = Expression.Parameter(types.ClrType);
				var memberAccess =
					Expression.MakeMemberAccess(parameterExpression, property);
				var negation = Expression.Not(memberAccess);
				var lambda = Expression.Lambda(negation, parameterExpression);
				types.QueryFilter = lambda;
			}
		}

		/// <inheritdoc />
		public override int SaveChanges()
		{
			UpdateSoftDeleted();
			return base.SaveChanges();
		}

		/// <inheritdoc />
		public override int SaveChanges(bool acceptAllChangesOnSuccess)
		{
			UpdateSoftDeleted();
			return base.SaveChanges(acceptAllChangesOnSuccess);
		}

		/// <inheritdoc />
		public override Task<int> SaveChangesAsync(
			CancellationToken cancellationToken = new CancellationToken())
		{
			UpdateSoftDeleted();
			return base.SaveChangesAsync(cancellationToken);
		}

		/// <inheritdoc />
		public override Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess,
			CancellationToken cancellationToken = new CancellationToken())
		{
			UpdateSoftDeleted();
			return base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
		}

		private void UpdateSoftDeleted()
		{
			foreach (var e in ChangeTracker.Entries())
			{
				if (e.Entity is ISoftDeletable sd && e.State == EntityState.Deleted)
				{
					e.State = EntityState.Modified;
					sd.IsDeleted = true;
				}
			}
		}
	}
}
