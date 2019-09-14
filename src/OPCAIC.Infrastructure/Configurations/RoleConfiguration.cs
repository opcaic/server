using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OPCAIC.Domain.Entities;

namespace OPCAIC.Infrastructure.Configurations
{
	public class RoleConfiguration : IEntityTypeConfiguration<Role>
	{
		/// <inheritdoc />
		public void Configure(EntityTypeBuilder<Role> builder)
		{
			builder.Property(r => r.Name).HasMaxLength(StringLengths.UserRoleName);
		}
	}
}