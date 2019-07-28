using OPCAIC.Infrastructure.Enums;

namespace OPCAIC.Infrastructure.Entities
{
	/// <summary>
	///     Aggregates how the submission did in the match.
	/// </summary>
	public class SubmissionMatchResult : EntityBase
	{
		/// <summary>
		///     Id of the match execution to which this record belongs.
		/// </summary>
		public long ExecutionId { get; set; }

		/// <summary>
		///     Execution of a match to which this record belongs.
		/// </summary>
		public virtual MatchExecution Execution { get; set; }

		/// <summary>
		///     Id of the submission.
		/// </summary>
		public long SubmissionId { get; set; }

		/// <summary>
		///     The submission to which this result belongs.
		/// </summary>
		public virtual Submission Submission { get; set; }

		/// <summary>
		///     Result of the compiler game module entry point of the compilation prior to the match execution.
		/// </summary>
		public GameModuleEntryPointResult CompilerResult { get; set; }

		/// <summary>
		///     Score of the submission in the match execution. Meaning depends on the format of the
		///     tournament.
		/// </summary>
		public double Score { get; set; }

		/// <summary>
		///     Additional key-value data received from the game module serialized as JSON object.
		/// </summary>
		public string AdditionalDataJson { get; set; }
	}
}