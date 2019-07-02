using System.IO;
using System.Threading.Tasks;
using OPCAIC.Messaging.Messages;
using OPCAIC.Worker.GameModules;

namespace OPCAIC.Worker.Services
{
	public interface IExecutionServices
	{
		DirectoryInfo GetWorkingDirectory(WorkMessageBase request);
		IGameModule GetGameModule(string game);
		Task DownloadSubmission(string serverPath, string localPath);
	}
}