using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using OPCAIC.Infrastructure.Entities;

namespace OPCAIC.Infrastructure.DbContexts
{
	public class DataContext : DbContext
	{
		public DataContext(DbContextOptions<DataContext> options)
			: base(options)
		{
		}

		public DbSet<Tournament> Tournaments { get; set; }
		public DbSet<Match> Matches { get; set; }
		public DbSet<MatchExecution> MatchExecutions { get; set; }
		public DbSet<Submission> Submissions { get; set; }
		public DbSet<SubmissionMatchResult> SubmissionsMatchResults { get; set; }
		public DbSet<User> Users { get; set; }
		public DbSet<UserRole> UserRoles { get; set; }

		/// <inheritdoc />
		protected override void OnModelCreating(ModelBuilder modelBuilder)
		{
			base.OnModelCreating(modelBuilder);
			AssertEntityTypesHaveCorrectBaseClass(modelBuilder.Model.GetEntityTypes());
			RegisterSoftDeleteQueryFilters(modelBuilder);

			// specify composite keys
			modelBuilder.Entity<Match>().HasKey(m => new {m.Id, m.TournamentId});
		}

		/// <summary>
		///   Application code relies on the fact that all entities derive from <see cref="IChangeTrackable" />,
		///   this function is just a sanity check.
		/// </summary>
		/// <param name="types"></param>
		[Conditional("DEBUG")]
		private void AssertEntityTypesHaveCorrectBaseClass(IEnumerable<IMutableEntityType> types)
		{
			foreach (var type in types)
				Debug.Assert(
					typeof(IChangeTrackable).IsAssignableFrom(type.ClrType),
					$"Type '{type.ClrType}' used in {nameof(DataContext)} does not derive from {typeof(IChangeTrackable)}.");
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
			OnSavingChanges();
			return base.SaveChanges();
		}

		/// <inheritdoc />
		public override int SaveChanges(bool acceptAllChangesOnSuccess)
		{
			OnSavingChanges();
			return base.SaveChanges(acceptAllChangesOnSuccess);
		}

		/// <inheritdoc />
		public override Task<int> SaveChangesAsync(
			CancellationToken cancellationToken = new CancellationToken())
		{
			OnSavingChanges();
			return base.SaveChangesAsync(cancellationToken);
		}

		/// <inheritdoc />
		public override Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess,
			CancellationToken cancellationToken = new CancellationToken())
		{
			OnSavingChanges();
			return base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
		}

		private void OnSavingChanges()
		{
			var now = DateTime.Now;

			foreach (var e in ChangeTracker.Entries())
			{
				if (e.Entity is ISoftDeletable sd && e.State == EntityState.Deleted)
				{
					// process soft delete condition
					e.State = EntityState.Modified;
					sd.IsDeleted = true;
				}

				if (e.Entity is IChangeTrackable eb)
				{
					// update timestamps
					switch (e.State)
					{
						case EntityState.Added:
							eb.Created = eb.Updated = now;
							break;
						case EntityState.Modified:
							eb.Updated = now;
							break;
					}
				}
			}
		}
	}
}
