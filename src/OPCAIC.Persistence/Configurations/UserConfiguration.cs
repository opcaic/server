using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OPCAIC.Domain.Entities;

namespace OPCAIC.Infrastructure.Configurations
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
		}
	}
}