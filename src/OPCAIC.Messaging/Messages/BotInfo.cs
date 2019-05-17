using System;

namespace OPCAIC.Messaging.Messages
{
	/// <summary>
	///   Class giving information about the bots in a match.
	/// </summary>
	[Serializable]
	public class BotInfo
	{
		/// <summary>
		///   Path on server where the submission files for this bot are stored
		/// </summary>
		public string SubmissionPath { get; set; }
	}
}
