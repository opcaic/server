namespace OPCAIC.Services
{
	/// <summary>
	///     Defines methods for creating
	/// </summary>
	public interface IMatchTreeFactory
	{
		/// <summary>
		///     Returns a match tree for single-elimination brackets format with given number of
		///     competitors and optionally with an extra match for a single third place.
		/// </summary>
		/// <param name="participants">Number of participants in the tournament.</param>
		/// <param name="singleThirdPlace">Flag whether extra match for a single third place is needed.</param>
		/// <returns></returns>
		SingleEliminationTree GetSingleEliminationTree(int participants, bool singleThirdPlace);

		/// <summary>
		///     Returns a match tree for double-elimination brackets format with given number of
		///     competitors.
		/// </summary>
		/// <param name="participants">Number of participants in the tournament.</param>
		/// <returns></returns>
		DoubleEliminationTree GetDoubleEliminationTree(int participants);
	}
}