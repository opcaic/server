using System.IO;

namespace OPCAIC.Worker.Services
{
	/// <summary>
	///     Working information about submission for evaluation purposes.
	/// </summary>
	public class SubmissionInfo
	{
		/// <summary>
		///     Index of the submission (position in the match)
		/// </summary>
		public int Index { get; set; }

		/// <summary>
		///     Directory with the source files for this submission.
		/// </summary>
		public DirectoryInfo SourceDirectory { get; set; }

		/// <summary>
		///     Directory with processed files which can be run.
		/// </summary>
		public DirectoryInfo BinaryDirectory { get; set; }
	}
}