namespace OPCAIC.Domain.Entities
{
	public class UserTournament : EntityBase
	{
		public long UserId { get; set; }

		public long TournamentId { get; set; }
	}
}