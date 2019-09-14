namespace OPCAIC.Domain.Entities
{
	public class TournamentParticipant : Entity
	{
		public long TournamentId { get; set; }

		public string Email { get; set; }

		public virtual Tournament Tournament { get; set; }
	}
}