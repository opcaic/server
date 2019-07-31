namespace OPCAIC.GameModules.Interface
{
	/// <summary>
	///     Represents full result of the executor stage of the game module.
	/// </summary>
	public class ExecutorResult : GameModuleResult
	{
		/// <summary>
		///     Results of the match.
		/// </summary>
		public MatchResult MatchResult { get; set; }
	}
}