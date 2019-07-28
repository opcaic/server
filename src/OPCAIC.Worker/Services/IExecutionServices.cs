using System.IO;
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
		///     Archives the contents of the given directory.
		/// </summary>
		/// <param name="taskDirectory">Directory to be archived.</param>
		void ArchiveDirectory(DirectoryInfo taskDirectory);
	}
}