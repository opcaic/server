namespace OPCAIC.Services
{
	/// <summary>
	///     Type of the link inside a match tree.
	/// </summary>
	public enum MatchTreeLinkType
	{
		/// <summary>
		///     Link concerns the winner of the linked match.
		/// </summary>
		Winner,

		/// <summary>
		///     Link concerns the looser of the linked match.
		/// </summary>
		Looser,

		/// <summary>
		///     Link concerns a fixed player identified by a seed number.
		/// </summary>
		Seed
	}
}