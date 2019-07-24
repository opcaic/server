using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using OPCAIC.Worker.Services;

namespace OPCAIC.Worker.GameModules
{
	//TODO: move this to separate project so it can be easily referenced?
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
		///     Invokes the checker entry point to check the submission.
		/// </summary>
		/// <param name="submission">Submission to check.</param>
		/// <param name="outputDir">Path to the directory with result files.</param>
		/// <param name="cancellationToken">Cancellation token used to cancel the request prematurely.</param>
		Task<CheckerResult> Check(SubmissionInfo submission, string outputDir,
			CancellationToken cancellationToken);

		/// <summary>
		///     Invokes the compiler entry point to compile the submission into executable.
		/// </summary>
		/// <param name="submission">Submission to compile.</param>
		/// <param name="outputDir"> Path to the directory with result files.</param>
		/// <param name="cancellationToken">Cancellation token used to cancel the request prematurely.</param>
		Task<CompilerResult> Compile(SubmissionInfo submission, string outputDir,
			CancellationToken cancellationToken);

		/// <summary>
		///     Invokes the validator entry point to validate the results of the compilation.
		/// </summary>
		/// <param name="submission">Submission to validate.</param>
		/// <param name="outputDir">Path to the directory with result files.</param>
		/// <param name="cancellationToken">Cancellation token used to cancel the request prematurely.</param>
		Task<ValidatorResult> Validate(SubmissionInfo submission, string outputDir,
			CancellationToken cancellationToken);

		/// <summary>
		///     Invokes the executor entry point to execute the match.
		/// </summary>
		/// <param name="submissions">Submissions to participate in the match.</param>
		/// <param name="outputDir">Path to the directory with result files.</param>
		/// <param name="cancellationToken">Cancellation token used to cancel the request prematurely.</param>
		Task<ExecutorResult> Execute(IEnumerable<SubmissionInfo> submissions, string outputDir,
			CancellationToken cancellationToken);

		/// <summary>
		///     Invokes the Cleaner entry point to reset the game module after a crash.
		/// </summary>
		/// <param name="cancellationToken">Cancellation token used to cancel the request prematurely.</param>
		Task Clean(CancellationToken cancellationToken);
	}
}