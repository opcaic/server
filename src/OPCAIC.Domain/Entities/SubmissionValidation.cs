﻿using System;
using OPCAIC.Domain.Enums;

namespace OPCAIC.Domain.Entities
{
	/// <summary>
	///     Result of a validation process of a submission.
	/// </summary>
	public class SubmissionValidation : Entity, IWorkerJob
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
		public EntryPointResult CheckerResult { get; set; }

		/// <summary>
		///     Result of the compiler game module entry point.
		/// </summary>
		public EntryPointResult CompilerResult { get; set; }

		/// <summary>
		///     Result of the validator game module entry point.
		/// </summary>
		public EntryPointResult ValidatorResult { get; set; }

		/// <inheritdoc />
		public Guid JobId { get; set; }

		/// <inheritdoc />
		public WorkerJobState State { get; set; }

		/// <inheritdoc />
		public DateTime? Executed { get; set; }

		/// <inheritdoc />
		public string Exception { get; set; }
	}
}