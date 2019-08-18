using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OPCAIC.Infrastructure.Entities;

namespace OPCAIC.Infrastructure.DbContexts
{
	public class DataContext : IdentityDbContext<User, Role, long>
	{
		public DataContext(DbContextOptions<DataContext> options)
			: base(options)
		{
		}

		public DbSet<Email> Emails { get; set; }
		public DbSet<EmailTemplate> EmailTemplates { get; set; }
		public DbSet<Game> Games { get; set; }
		public DbSet<Tournament> Tournaments { get; set; }
		public DbSet<Match> Matches { get; set; }
		public DbSet<Document> Documents { get; set; }
		public DbSet<MatchExecution> MatchExecutions { get; set; }
		public DbSet<Submission> Submissions { get; set; }
		public DbSet<SubmissionValidation> SubmissionValidations { get; set; }
		public DbSet<SubmissionMatchResult> SubmissionsMatchResults { get; set; }
		public DbSet<UserTournament> UserTournaments { get; set; }

		/// <inheritdoc />
		protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
		{
			optionsBuilder.UseLazyLoadingProxies();
		}

		/// <inheritdoc />
		protected override void OnModelCreating(ModelBuilder modelBuilder)
		{
			base.OnModelCreating(modelBuilder);
			RegisterSoftDeleteQueryFilters(modelBuilder);
			ConfigureEntities(modelBuilder);
		}

		private static void ConfigureEntities(ModelBuilder modelBuilder)
		{
			var method = typeof(ModelBuilder)
				.GetMethods(BindingFlags.Instance | BindingFlags.Public)
				.Single(m =>
					m.Name == nameof(modelBuilder.Entity) &&
					m.ContainsGenericParameters &&
					m.GetParameters().Length == 1);

			// call modelBuilder.Entity<TEntity>(TEntity.OnModelCreating) for all types which have this method.
			foreach (var type in modelBuilder.Model.GetEntityTypes())
			{
				var clrType = type.ClrType;
				var configureMethod = clrType.GetMethod(nameof(OnModelCreating),
					BindingFlags.Static | BindingFlags.DeclaredOnly | BindingFlags.NonPublic);

				if (configureMethod == null)
				{
					// no special configuration for this type
					continue;
				}

				// Construct Action<EntityModelBuilder<TEntity>> argument type for the Entity call
				var delegateType = typeof(Action<>).MakeGenericType(
					typeof(EntityTypeBuilder<>).MakeGenericType(clrType));

				// actual method call
				method.MakeGenericMethod(clrType).Invoke(
					modelBuilder,
					new object[] {configureMethod.CreateDelegate(delegateType)});
			}
		}

		private static void RegisterSoftDeleteQueryFilters(ModelBuilder modelBuilder)
		{
			var property = typeof(ISoftDeletable).GetProperty(nameof(ISoftDeletable.IsDeleted));

			foreach (var type in modelBuilder.Model.GetEntityTypes()
				.Where(t => typeof(ISoftDeletable).IsAssignableFrom(t.ClrType)))
			{
				// construct (e => !e.isDeleted) expression
				var parameterExpression = Expression.Parameter(type.ClrType);
				var memberAccess =
					Expression.MakeMemberAccess(parameterExpression, property);
				var negation = Expression.Not(memberAccess);
				var lambda = Expression.Lambda(negation, parameterExpression);
				type.QueryFilter = lambda;
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