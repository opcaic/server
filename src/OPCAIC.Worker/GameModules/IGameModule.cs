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
		/// <param name="outputDir">Path to the directory with result files.</param> 
		/// <param name="inputSrcDir">Path to the directory with bot source files</param>
		CheckerResult Check(string inputSrcDir, string outputDir);

		/// <summary>
		///   Invokes the compiler entry point to compile the submission into executable.
		/// </summary>
		/// <param name="outputDir"> Path to the directory with result files.</param>
		/// <param name="inputSrcDir">Path to the directory with bot source files.</param>
		/// <param name="outputBinDir">Path to the directory in which to store the compiled bot.</param>
		CompilerResult Compile(string inputSrcDir, string outputBinDir, string outputDir);

		/// <summary>
		///   Invokes the validator entry point to validate the results of the compilation.
		/// </summary>
		/// <param name="outputDir">Path to the directory with result files.</param>
		/// <param name="inputBinDir">Path to the directory with the compiled bot.</param>
		ValidatorResult Validate(string inputBinDir, string outputDir);

		/// <summary>
		///   Invokes the executor entry point to execute the match.
		/// </summary>
		/// <param name="inputDirs">Paths to the directories with input files.</param>
		/// <param name="outputDir">Path to the directory in which to store the result files.</param>
		ExecutorResult Execute(string[] inputDirs, string outputDir);

		/// <summary>
		///   Invokes the Cleaner entry point to reset the game module after a crash.
		/// </summary>
		void Clean();
	}
}