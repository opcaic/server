namespace OPCAIC.Domain.Enumsawef
{
	/// <summary>
	///     Represents types of games based on the number of their players.
	/// </summary>
	public enum GameType
	{
		/// <summary>
		///     Unknown type, should never occur.
		/// </summary>
		Unknown,

		/// <summary>
		///     Games for one player.
		/// </summary>
		SinglePlayer,

		/// <summary>
		///     Games for two players.
		/// </summary>
		TwoPlayer,

		/// <summary>
		///     Games for N >= 3 players.
		/// </summary>
		Multiplayer
	}
}