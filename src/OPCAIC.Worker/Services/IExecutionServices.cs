using System.IO;
using OPCAIC.Messaging.Messages;

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
		///     Archives the contents of the given directory.
		/// </summary>
		/// <param name="taskDirectory">Directory to be archived.</param>
		void ArchiveDirectory(DirectoryInfo taskDirectory);
	}
}