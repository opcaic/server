using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace OPCAIC.Infrastructure.Entities
{
	public class TournamentManager
	{
		public long UserId { get; set; }

		public virtual User User { get; set; }

		public long TournamentId { get; set; }

		public virtual Tournament Tournament { get; set; }

		internal static void OnModelCreating(EntityTypeBuilder<TournamentManager> builder)
		{
			// mapping table, make sure the same mapping does not exist twice
			builder.HasKey(nameof(UserId), nameof(TournamentId));
		}
	}
}