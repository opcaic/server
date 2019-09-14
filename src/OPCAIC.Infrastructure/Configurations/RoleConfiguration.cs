using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OPCAIC.Infrastructure.Entities;

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