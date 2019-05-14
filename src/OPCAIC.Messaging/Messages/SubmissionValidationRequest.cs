using System;

namespace OPCAIC.Messaging.Messages
{
	[Serializable]
	public class SubmissionValidationRequest : WorkMessageBase
	{
		public string SubmissionPath { get; set; }
	}
}