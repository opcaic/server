namespace OPCAIC.Messaging.Messages
{
	/// <summary>
	///     Message with result of submission validation process.
	/// </summary>
	public class SubmissionValidationResult : ReplyMessageBase
	{
		/// <summary>
		///     Result of the task checking validity of the submission.
		/// </summary>
		public SubTaskResult CheckerResult { get; set; }

		/// <summary>
		///     Result of the task compiling the submission into executable binaries.
		/// </summary>
		public SubTaskResult CompilerResult { get; set; }

		/// <summary>
		///     Result of the task validating the compiled binaries.
		/// </summary>
		public SubTaskResult ValidatorResult { get; set; }
	}
}