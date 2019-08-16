namespace OPCAIC.Infrastructure.Entities
{
	public class UserParticipation : EntityBase
	{
		/// <summary>
		///     The id of the match of this participation.
		/// </summary>
		public long MatchId { get; set; }

		/// <summary>
		///     The match the user participates in.
		/// </summary>
		public virtual Match Match { get; set; }

		/// <summary>
		///     The id of the user participating.
		/// </summary>
		public long UserId { get; set; }

		/// <summary>
		///     The user participating.
		/// </summary>
		public virtual User User { get; set; }
	}
}