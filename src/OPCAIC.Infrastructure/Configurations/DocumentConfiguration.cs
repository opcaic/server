using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OPCAIC.Infrastructure.Entities;

namespace OPCAIC.Infrastructure.Configurations
{
	public class DocumentConfiguration : IEntityTypeConfiguration<Document>
	{
		/// <inheritdoc />
		public void Configure(EntityTypeBuilder<Document> builder)
		{
			builder.Property(e => e.Name).IsRequired().HasMaxLength(StringLengths.DocumentName);
		}
	}

	public class UserConfiguration : IEntityTypeConfiguration<User>
	{
		/// <inheritdoc />
		public void Configure(EntityTypeBuilder<User> builder)
		{
			builder.Property(e => e.FirstName).HasMaxLength(StringLengths.UserFirstName);
			builder.Property(e => e.LastName).HasMaxLength(StringLengths.UserLastName);
			builder.Property(u => u.UserName).IsRequired().HasMaxLength(StringLengths.UserName);
			builder.Property(u => u.Email).IsRequired().HasMaxLength(StringLengths.UserEmail);
			builder.Property(u => u.PasswordHash).HasMaxLength(StringLengths.PasswordHash)
				.IsRequired();
		}
	}

	public class GameConfiguration : IEntityTypeConfiguration<Game>
	{
		/// <inheritdoc />
		public void Configure(EntityTypeBuilder<Game> builder)
		{
			builder.Property(e => e.Name).IsRequired().HasMaxLength(StringLengths.GameName);
			builder.Property(e => e.Key).IsRequired().HasMaxLength(StringLengths.GameKey);
		}
	}

	public class RoleConfiguration : IEntityTypeConfiguration<Role>
	{
		/// <inheritdoc />
		public void Configure(EntityTypeBuilder<Role> builder)
		{
			builder.Property(r => r.Name).HasMaxLength(StringLengths.UserRoleName);
		}
	}

	public class SubmissionParticipationConfiguration : IEntityTypeConfiguration<SubmissionParticipation>
	{
		/// <inheritdoc />
		public void Configure(EntityTypeBuilder<SubmissionParticipation> builder)
		{
			// since this is just a mapping table, no need for dedicated id.
			builder.HasKey(nameof(SubmissionParticipation.MatchId), nameof(SubmissionParticipation.SubmissionId), nameof(SubmissionParticipation.Order));
		}
	}

	public class TournamentConfiguration : IEntityTypeConfiguration<Tournament>
	{
		/// <inheritdoc />
		public void Configure(EntityTypeBuilder<Tournament> builder)
		{
			builder.Property(e => e.Name).IsRequired().HasMaxLength(StringLengths.TournamentName);
		}
	}

	public class TournamentManagerConfiguration : IEntityTypeConfiguration<TournamentManager>
	{
		/// <inheritdoc />
		public void Configure(EntityTypeBuilder<TournamentManager> builder)
		{
			// mapping table, make sure the same mapping does not exist twice
			builder.HasKey(nameof(TournamentManager.UserId), nameof(TournamentManager.TournamentId));
		}
	}
}