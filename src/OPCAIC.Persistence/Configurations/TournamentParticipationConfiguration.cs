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
			// mapping table, does not need a dedicated key
			builder.HasKey(nameof(TournamentParticipation.TournamentId),
				nameof(TournamentParticipation.UserId));

			builder.HasMany(e => e.Submissions).WithOne(e => e.TournamentParticipation)
				.HasForeignKey(nameof(Submission.TournamentId), nameof(Submission.AuthorId));
		}
	}
}