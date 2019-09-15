using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OPCAIC.Domain.Entities;

namespace OPCAIC.Infrastructure.Configurations
{
	public class SubmissionParticipationConfiguration : IEntityTypeConfiguration<SubmissionParticipation>
	{
		/// <inheritdoc />
		public void Configure(EntityTypeBuilder<SubmissionParticipation> builder)
		{
			// since this is just a mapping table, no need for dedicated id.
			builder.HasKey(nameof(SubmissionParticipation.MatchId), nameof(SubmissionParticipation.SubmissionId), nameof(SubmissionParticipation.Order));
		}
	}
}