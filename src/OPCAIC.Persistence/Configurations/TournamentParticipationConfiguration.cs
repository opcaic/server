using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OPCAIC.Domain.Entities;

namespace OPCAIC.Persistence.Configurations
{
	public class TournamentParticipationConfiguration : IEntityTypeConfiguration<TournamentParticipation>
	{
		/// <inheritdoc />
		public void Configure(EntityTypeBuilder<TournamentParticipation> builder)
		{
			builder.HasMany(e => e.Submissions).WithOne(e => e.TournamentParticipation).IsRequired();
		}
	}
}