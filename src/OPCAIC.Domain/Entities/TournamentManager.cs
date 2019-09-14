namespace OPCAIC.Infrastructure.Entities
{
	public class TournamentManager
	{
		public long UserId { get; set; }

		public virtual User User { get; set; }

		public long TournamentId { get; set; }

		public virtual Tournament Tournament { get; set; }
	}
}