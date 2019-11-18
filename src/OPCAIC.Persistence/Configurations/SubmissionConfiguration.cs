using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OPCAIC.Domain.Entities;

namespace OPCAIC.Persistence.Configurations
{
	public class SubmissionConfiguration : IEntityTypeConfiguration<Submission>
	{
		/// <inheritdoc />
		public void Configure(EntityTypeBuilder<Submission> builder)
		{
			builder.HasOne(e => e.TournamentParticipation).WithMany(e => e.Submissions);

			builder.HasMany(e => e.Validations).WithOne(e => e.Submission)
				.HasForeignKey(e => e.SubmissionId);
		}
	}
}