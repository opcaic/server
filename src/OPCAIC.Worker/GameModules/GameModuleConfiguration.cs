namespace OPCAIC.Worker.GameModules
{
	/// <summary>
	///     Configuration of the external game module.
	/// </summary>
	public class GameModuleConfiguration
	{
		/// <summary>
		///     Configuration of the checker entry point.
		/// </summary>
		public EntryPointConfiguration Checker { get; set; }

		/// <summary>
		///     Configuration of the compiler entry point.
		/// </summary>
		public EntryPointConfiguration Compiler { get; set; }

		/// <summary>
		///     Configuration of the validator entry point.
		/// </summary>
		public EntryPointConfiguration Validator { get; set; }

		/// <summary>
		///     Configuration of the Executor entry point.
		/// </summary>
		public EntryPointConfiguration Executor { get; set; }
	}
}