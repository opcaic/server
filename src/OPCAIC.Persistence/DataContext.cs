using System;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using OPCAIC.Common;
using OPCAIC.Domain.Entities;
using OPCAIC.Domain.Infrastructure;
using OPCAIC.Persistence.ValueConverters;

namespace OPCAIC.Persistence
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

		/// <inheritdoc />
		protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
		{
			// Uncomment to make client-side query evaluation throw (for debug purposes)
//			optionsBuilder.ConfigureWarnings(w
//				=> w.Throw(RelationalEventId.QueryClientEvaluationWarning));
		}

		/// <inheritdoc />
		protected override void OnModelCreating(ModelBuilder modelBuilder)
		{
			base.OnModelCreating(modelBuilder);
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(DataContext).Assembly);
            ConfigureEntities(modelBuilder);
			ConfigureEnumerations(modelBuilder);
		}

		private static void ConfigureEnumerations(ModelBuilder modelBuilder)
		{
			foreach (var entityType in modelBuilder.Model.GetEntityTypes())
			{
				var builder = modelBuilder.Entity(entityType.ClrType);

				foreach (var property in entityType.ClrType.GetProperties())
				{
					Type converterType;
					if (Enumeration.IsEnumerationType(property.PropertyType))
					{
						converterType = typeof(EfEnumerationConverter<>);
					}
					else if (property.PropertyType.IsEnum)
					{
						converterType = typeof(EnumToStringConverter<>);
					}
					else
					{
						continue;
					}

					builder.Property(property.Name).HasConversion(
						(ValueConverter)Activator.CreateInstance(
							converterType.MakeGenericType(property.PropertyType), new[] { (object)null }));
				}
			}
		}

		private static void ConfigureEntities(ModelBuilder modelBuilder)
		{
			foreach (var type in modelBuilder.Model.GetEntityTypes())
			{
				var clrType = type.ClrType;

				if (typeof(Entity).IsAssignableFrom(clrType))
				{
					modelBuilder.Entity(clrType).Property<DateTime>(nameof(Entity.Updated))
						.IsConcurrencyToken();
				}
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
			var now = DateTime.UtcNow;

			foreach (var e in ChangeTracker.Entries())
			{
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