using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OPCAIC.Common;
using OPCAIC.Domain.Entities;

namespace OPCAIC.Persistence.Configurations
{
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
			builder.Property(e => e.Organization).HasMaxLength(StringLengths.Organization);

			builder.HasMany(u => u.ManagerOfTournaments)
				.WithOne(m => m.User)
				.HasForeignKey(m => m.UserId)
				.OnDelete(DeleteBehavior.Cascade);

			builder.HasMany(u => u.TournamentParticipations)
				.WithOne(m => m.User)
				.HasForeignKey(m => m.UserId)
				.OnDelete(DeleteBehavior.SetNull);

			builder.HasMany(u => u.TournamentInvitations)
				.WithOne(m => m.User)
				.HasForeignKey(m => m.UserId)
				.OnDelete(DeleteBehavior.Cascade);

			builder.HasMany(u => u.Submissions)
				.WithOne(m => m.Author)
				.HasForeignKey(m => m.AuthorId)
				.OnDelete(DeleteBehavior.SetNull);

			builder.HasMany(u => u.OwnedTournaments)
				.WithOne(m => m.Owner)
				.HasForeignKey(m => m.OwnerId)
				.OnDelete(DeleteBehavior.Restrict);
		}
	}
}