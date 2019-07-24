using System.IO;
using System.Threading.Tasks;
using OPCAIC.Messaging.Messages;
using OPCAIC.Worker.GameModules;

namespace OPCAIC.Worker.Services
{
	public interface IExecutionServices
	{
		/// <summary>
		///     Gets working directory for the given work task.
		/// </summary>
		/// <param name="request">The message defining the task.</param>
		/// <returns></returns>
		DirectoryInfo GetWorkingDirectory(WorkMessageBase request);

		/// <summary>
		///     Gets the game identified by the given name.
		/// </summary>
		/// <param name="game">The string name of the game</param>
		/// <returns></returns>
		IGameModule GetGameModule(string game);

		/// <summary>
		///     Downloads submission from the server to the given path.
		/// </summary>
		/// <param name="serverPath">Path of the submission on the server.</param>
		/// <param name="localPath">Path to directory where the submission is to be downloaded.</param>
		/// <returns></returns>
		Task DownloadSubmission(string serverPath, string localPath);

		/// <summary>
		///     Uploads the contents of the folder as results of the task.
		/// </summary>
		/// <param name="request">Original request message.</param>
		/// <param name="outputDirectory">Directory with output files to be uploaded.</param>
		/// <returns></returns>
		Task UploadResults(WorkMessageBase request, DirectoryInfo outputDirectory);

		/// <summary>
		///     Archives the contents of the given directory.
		/// </summary>
		/// <param name="taskDirectory">Directory to be archived.</param>
		void ArchiveDirectory(DirectoryInfo taskDirectory);
	}
}