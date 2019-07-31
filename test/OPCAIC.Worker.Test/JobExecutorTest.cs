using System.IO;
using System.Threading;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using OPCAIC.GameModules.Interface;
using OPCAIC.Messaging.Messages;
using OPCAIC.TestUtils;
using OPCAIC.Worker.Config;
using OPCAIC.Worker.GameModules;
using OPCAIC.Worker.Services;
using Xunit.Abstractions;

namespace OPCAIC.Worker.Test
{
	public abstract class JobExecutorTest : ServiceTestBase
	{
		protected readonly Mock<IGameModule> GameModuleMock;
		protected readonly Mock<IDownloadService> DownloadServiceMock;
		protected readonly Mock<IExecutionServices> ExecutionServicesMock;

		/// <inheritdoc />
		protected JobExecutorTest(ITestOutputHelper output) : base(output)
		{
			ArchiveDirectory = NewDirectory();
			WorkingDirectory = NewDirectory();

			Services.Configure<ExecutionConfig>(cfg =>
			{
				cfg.ArchiveDirectoryRoot = ArchiveDirectory.FullName;
				cfg.WorkingDirectoryRoot = WorkingDirectory.FullName;
				cfg.MaxTaskTimeout = 10000;
			});

			GameModuleMock = Services.Mock<IGameModule>();
			ExecutionServicesMock = Services.Mock<IExecutionServices>();

			Services.Mock<IGameModuleRegistry>()
				.Setup(s => s.FindGameModule(It.IsAny<string>()))
				.Returns(GameModuleMock.Object);

			ExecutionServicesMock
				.Setup(s => s.GetWorkingDirectory(It.IsAny<WorkMessageBase>()))
				.Returns(NewDirectory);

			DownloadServiceMock = Services.Mock<IDownloadService>();
			DownloadServiceMock
				.Setup(s => s.DownloadSubmission(It.IsAny<long>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
				.Callback(MockExtensions.WriteRandomContentToTargetFolder);
		}

		private DirectoryInfo ArchiveDirectory { get; }
		private DirectoryInfo WorkingDirectory { get; }
	}
}