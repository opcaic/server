namespace OPCAIC.Domain.Enums
{
	/// <summary>
	///     Simplified submission validation state.
	/// </summary>
	public enum SubmissionValidationState
	{
		/// <summary>
		///     Submission is queued for validation.
		/// </summary>
		Queued,

		/// <summary>
		///     Validation finished successfully.
		/// </summary>
		Valid,

		/// <summary>
		///     Validation finished with negative result.
		/// </summary>
		Invalid,

		/// <summary>
		///     Validation failed.
		/// </summary>
		Error,

		/// <summary>
		///     Validation has been cancelled.
		/// </summary>
		Cancelled
	}
}