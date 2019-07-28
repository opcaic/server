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
		///   Id of the submission of this bot.
		/// </summary>
		public string SubmissionId { get; set; }
	}
}
