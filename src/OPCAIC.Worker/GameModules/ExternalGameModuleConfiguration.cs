namespace OPCAIC.Worker.GameModules
{
	/// <summary>
	///     Configuration of the external game module.
	/// </summary>
	public class ExternalGameModuleConfiguration
	{
		/// <summary>
		///     Configuration of the checker entry point.
		/// </summary>
		public ExternalEntryPointConfiguration Checker { get; set; }

		/// <summary>
		///     Configuration of the compiler entry point.
		/// </summary>
		public ExternalEntryPointConfiguration Compiler { get; set; }

		/// <summary>
		///     Configuration of the validator entry point.
		/// </summary>
		public ExternalEntryPointConfiguration Validator { get; set; }

		/// <summary>
		///     Configuration of the Executor entry point.
		/// </summary>
		public ExternalEntryPointConfiguration Executor { get; set; }

		/// <summary>
		///     Configuration of the Cleanup entry point.
		/// </summary>
		public ExternalEntryPointConfiguration Cleanup { get; set; }
	}
}