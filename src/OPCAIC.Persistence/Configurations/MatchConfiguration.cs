using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OPCAIC.Domain.Entities;

namespace OPCAIC.Persistence.Configurations
{
	public class MatchConfiguration : IEntityTypeConfiguration<Match>
	{
		/// <inheritdoc />
		public void Configure(EntityTypeBuilder<Match> builder)
		{
			builder.HasMany(e => e.Executions).WithOne(e => e.Match)
				.HasForeignKey(e => e.MatchId);
		}
	}
}