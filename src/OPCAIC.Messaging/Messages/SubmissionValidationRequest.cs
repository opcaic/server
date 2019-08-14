namespace OPCAIC.Messaging.Messages
{
	/// <summary>
	///     Message requesting validation of a submission.
	/// </summary>
	public class SubmissionValidationRequest : WorkMessageBase
	{
		/// <summary>
		///     Id of the validated submission.
		/// </summary>
		public long SubmissionId { get; set; }

		/// <summary>
		///     Id of the validation.
		/// </summary>
		public long ValidationId { get; set; }
	}
}