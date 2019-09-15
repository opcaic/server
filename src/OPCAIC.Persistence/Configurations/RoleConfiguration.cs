using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OPCAIC.Common;
using OPCAIC.Domain.Entities;

namespace OPCAIC.Persistence.Configurations
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