namespace OPCAIC.GameModules.Interface
{
	/// <summary>
	///     Represents full result of a stage of the game module.
	/// </summary>
	public abstract class GameModuleResult
	{
		public GameModuleEntryPointResult EntryPointResult { get; set; }
	}
}