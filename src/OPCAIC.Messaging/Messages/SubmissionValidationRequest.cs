namespace OPCAIC.Messaging.Messages
{
	/// <summary>
	///   Message requesting validation of a submission.
	/// </summary>
	public class SubmissionValidationRequest : WorkMessageBase
	{
		/// <summary>
		///   Path to the submission files
		/// </summary>
		public string Path { get; set; }
	}
}
