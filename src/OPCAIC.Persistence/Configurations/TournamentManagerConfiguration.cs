using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OPCAIC.Domain.Entities;

namespace OPCAIC.Persistence.Configurations
{
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