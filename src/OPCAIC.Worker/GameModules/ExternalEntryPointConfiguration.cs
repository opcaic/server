namespace OPCAIC.Worker.GameModules
{
	/// <summary>
	///     Configuration of a single entry point of a module
	/// </summary>
	public class ExternalEntryPointConfiguration
	{
		/// <summary>
		///     Path to the executable to start.
		/// </summary>
		public string Executable { get; set; }
		/// <summary>
		///     Additional arguments to be passed to the above executable, these will be passed first.
		/// </summary>
		public string[] Arguments { get; set; }
	}
}