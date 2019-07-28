using System.IO;

namespace OPCAIC.GameModules.Interface
{
	/// <summary>
	///     Working information about submission for evaluation purposes.
	/// </summary>
	public class BotInfo
	{
		/// <summary>
		///     Index of the bot (position in the match)
		/// </summary>
		public int Index { get; set; }

		/// <summary>
		///     Directory with the source files for this bot.
		/// </summary>
		public DirectoryInfo SourceDirectory { get; set; }

		/// <summary>
		///     Directory with processed files which can be run.
		/// </summary>
		public DirectoryInfo BinaryDirectory { get; set; }
	}
}