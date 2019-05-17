namespace OPCAIC.Worker.GameModules
{
	//TODO: move this to separate project so it can be easily referenced?
	/// <summary>
	///   Provides methods for invoking the entry points of a game module.
	/// </summary>
	public interface IGameModule
	{
		/// <summary>
		///   Name of the game this module executes.
		/// </summary>
		string GameName { get; }

		/// <summary>
		///   Invokes the checker entry point to check the submission.
		/// </summary>
		/// <param name="inputDir">Path to the directory with input files.</param>
		/// <param name="outputDir">Path to the directory in which to store the output files.</param>
		void Check(string inputDir, string outputDir);

		/// <summary>
		///   Invokes the compiler entry point to compile the submission into executable.
		/// </summary>
		/// <param name="inputDir">Path to the directory with input files.</param>
		/// <param name="outputDir">Path to the directory in which to store the output files.</param>
		void Compile(string inputDir, string outputDir);

		/// <summary>
		///   Invokes the validator entry point to validate the results of the compilation.
		/// </summary>
		/// <param name="inputDir">Path to the directory with input files.</param>
		/// <param name="outputDir">Path to the directory in which to store the output files.</param>
		void Validate(string inputDir, string outputDir);

		/// <summary>
		///   Invokes the executor entry point to execute the match.
		/// </summary>
		/// <param name="inputDir">Path to the directory with input files.</param>
		/// <param name="outputDir">Path to the directory in which to store the output files.</param>
		void Execute(string inputDir, string outputDir);

		/// <summary>
		///   Invokes the Cleaner entry point to reset the game module after a crash.
		/// </summary>
		void Clean();
	}
}
