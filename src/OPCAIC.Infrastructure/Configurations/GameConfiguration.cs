using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OPCAIC.Infrastructure.Entities;

namespace OPCAIC.Infrastructure.Configurations
{
	public class GameConfiguration : IEntityTypeConfiguration<Game>
	{
		/// <inheritdoc />
		public void Configure(EntityTypeBuilder<Game> builder)
		{
			builder.Property(e => e.Name).IsRequired().HasMaxLength(StringLengths.GameName);
			builder.Property(e => e.Key).IsRequired().HasMaxLength(StringLengths.GameKey);
		}
	}
}