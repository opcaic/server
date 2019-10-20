using OPCAIC.Domain.Enums;

namespace OPCAIC.Domain.Entities
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
		///     Order of the submission in the match.
		/// </summary>
		public int Order { get; set; }

		/// <summary>
		///     The submission to which this result belongs.
		/// </summary>
		public virtual Submission Submission { get; set; }

		/// <summary>
		///     Result of the compiler game module entry point of the compilation prior to the match execution.
		/// </summary>
		public EntryPointResult CompilerResult { get; set; }

		/// <summary>
		///     Score of the submission in the match execution. Meaning depends on the format of the
		///     tournament.
		/// </summary>
		public double Score { get; set; }

		/// <summary>
		///     Flag whether the submission misbehaved (invalid input or runtime crash).
		/// </summary>
		public bool Crashed { get; set; }

		/// <summary>
		///     Additional key-value data received from the game module serialized as JSON object.
		/// </summary>
		public string AdditionalData { get; set; }
	}
}