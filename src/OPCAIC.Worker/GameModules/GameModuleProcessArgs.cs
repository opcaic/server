using System.Collections.Generic;
using System.IO;

namespace OPCAIC.Worker.GameModules
{
	/// <summary>
	///     Simplified set of arguments used to start a process.
	/// </summary>
	public class GameModuleProcessArgs
	{
		/// <summary>
		///     Working directory of the process.
		/// </summary>
		public string WorkingDirectory { get; set; }

		/// <summary>
		///     Entry point to be executed.
		/// </summary>
		public ExternalEntryPointConfiguration EntryPoint { get; set; }

		/// <summary>
		///     Arguments to the program being run.
		/// </summary>
		public List<string> Arguments { get; set; } = new List<string>();

		/// <summary>
		///     Instance of <see cref="TextWriter" /> class to which write stdout stream.
		/// </summary>
		public TextWriter StandardOutput { get; set; } = TextWriter.Null;

		/// <summary>
		///     Instance of <see cref="TextWriter" /> class to which write stderr stream.
		/// </summary>
		public TextWriter StandardError { get; set; } = TextWriter.Null;
	}
}