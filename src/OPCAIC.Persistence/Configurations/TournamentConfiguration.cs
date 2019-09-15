using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OPCAIC.Common;
using OPCAIC.Domain.Entities;

namespace OPCAIC.Persistence.Configurations
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