using System;
using OPCAIC.Infrastructure.Enums;

namespace OPCAIC.Infrastructure.Entities
{
	/// <summary>
	///     Result of a validation process of a submission.
	/// </summary>
	public class SubmissionValidation : SoftDeletableEntity, IWorkerJob
	{
		/// <summary>
		///     Id of the validated submission.
		/// </summary>
		public long SubmissionId { get; set; }

		/// <summary>
		///     Submission which was being validated.
		/// </summary>
		public virtual Submission Submission { get; set; }

		/// <summary>
		///     Result of the checker game module entry point.
		/// </summary>
		public GameModuleEntryPointResult CheckerResult { get; set; }

		/// <summary>
		///     Result of the compiler game module entry point.
		/// </summary>
		public GameModuleEntryPointResult CompilerResult { get; set; }

		/// <summary>
		///     Result of the validator game module entry point.
		/// </summary>
		public GameModuleEntryPointResult ValidatorResult { get; set; }

		/// <inheritdoc />
		public Guid JobId { get; set; }

		/// <inheritdoc />
		public WorkerJobState State { get; set; }

		/// <inheritdoc />
		public DateTime? Finished { get; set; }
	}
}