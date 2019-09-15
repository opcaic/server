using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OPCAIC.Domain.Entities;

namespace OPCAIC.Infrastructure.Configurations
{
	public class TournamentConfiguration : IEntityTypeConfiguration<Tournament>
	{
		/// <inheritdoc />
		public void Configure(EntityTypeBuilder<Tournament> builder)
		{
			builder.Property(e => e.Name).IsRequired().HasMaxLength(StringLengths.TournamentName);
		}
	}
}