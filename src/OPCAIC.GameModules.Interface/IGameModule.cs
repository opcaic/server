using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace OPCAIC.GameModules.Interface
{
	/// <summary>
	///     Provides methods for invoking the entry points of a game module.
	/// </summary>
	public interface IGameModule
	{
		/// <summary>
		///     Name of the game this module executes.
		/// </summary>
		string GameName { get; }

		/// <summary>
		///     Invokes the checker entry point to check the bot.
		/// </summary>
		/// <param name="config">Configuration to be used.</param>
		/// <param name="bot">Submission to check.</param>
		/// <param name="outputDir">Path to the directory with result files.</param>
		/// <param name="cancellationToken">Cancellation token used to cancel the request prematurely.</param>
		Task<CheckerResult> Check(EntryPointConfiguration config, BotInfo bot,
			DirectoryInfo outputDir,
			CancellationToken cancellationToken);

		/// <summary>
		///     Invokes the compiler entry point to compile the bot into executable.
		/// </summary>
		/// <param name="config">Configuration to be used.</param>
		/// <param name="bot">Submission to compile.</param>
		/// <param name="outputDir"> Path to the directory with result files.</param>
		/// <param name="cancellationToken">Cancellation token used to cancel the request prematurely.</param>
		Task<CompilerResult> Compile(EntryPointConfiguration config, BotInfo bot,
			DirectoryInfo outputDir,
			CancellationToken cancellationToken);

		/// <summary>
		///     Invokes the validator entry point to validate the results of the compilation.
		/// </summary>
		/// <param name="config">Configuration to be used.</param>
		/// <param name="bot">Submission to validate.</param>
		/// <param name="outputDir">Path to the directory with result files.</param>
		/// <param name="cancellationToken">Cancellation token used to cancel the request prematurely.</param>
		Task<ValidatorResult> Validate(EntryPointConfiguration config, BotInfo bot,
			DirectoryInfo outputDir, CancellationToken cancellationToken);

		/// <summary>
		///     Invokes the executor entry point to execute the match.
		/// </summary>
		/// <param name="config">Configuration to be used.</param>
		/// <param name="submissions">Submissions to participate in the match.</param>
		/// <param name="outputDir">Path to the directory with result files.</param>
		/// <param name="cancellationToken">Cancellation token used to cancel the request prematurely.</param>
		Task<ExecutorResult> Execute(EntryPointConfiguration config,
			IEnumerable<BotInfo> submissions, DirectoryInfo outputDir,
			CancellationToken cancellationToken);

		/// <summary>
		///     Invokes the Cleaner entry point to reset the game module after a crash.
		/// </summary>
		/// <param name="config">Configuration to be used.</param>
		/// <param name="cancellationToken">Cancellation token used to cancel the request prematurely.</param>
		Task<CleanerResult> Clean(EntryPointConfiguration config, CancellationToken cancellationToken);
	}
}