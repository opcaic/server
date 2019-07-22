namespace OPCAIC.Infrastructure.Enums
{
	/// <summary>
	///   Strategy how to determine the best submission in a tournament.
	/// </summary>
	public enum TournamentRankingStrategy
	{
		/// <summary>
		///   Unknown, should never occur.
		/// </summary>
		Unknown,

		/// <summary>
		///   Players are ranked in descending order based on their score/rating.
		/// </summary>
		Maximum,

		/// <summary>
		///   Players are ranked in ascending order based on their score/rating.
		/// </summary>
		Minimum,
	}
}