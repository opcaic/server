using System;
using System.IO;
using GameModuleMock;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using OPCAIC.Messaging;
using OPCAIC.Messaging.Messages;
using OPCAIC.Worker.Config;
using OPCAIC.Worker.GameModules;
using OPCAIC.Worker.Services;
using Xunit;
using Xunit.Abstractions;

namespace OPCAIC.Worker.Test
{
	public class IntegrationTest : ExternalGameModuleTest
	{
		private readonly Mock<IExecutionServices> executionServicesMock;
		private readonly WorkerConnectorHelper connectorHelper;

		private DirectoryInfo ArchiveDirectory { get; }
		private DirectoryInfo WorkingDirectory { get; }

		/// <inheritdoc />
		public IntegrationTest(ITestOutputHelper output) : base(output)
		{
			ArchiveDirectory = NewDirectory();
			WorkingDirectory = NewDirectory();

			Services
				.AddTransient<
					IJobExecutor<SubmissionValidationRequest, SubmissionValidationResult>,
					SubmissionValidator>()
				.Configure<ExecutionConfig>(cfg =>
				{
					cfg.ArchiveDirectoryRoot = ArchiveDirectory.FullName;
					cfg.WorkingDirectoryRoot = WorkingDirectory.FullName;
					cfg.MaxTaskTimeout = 10000;
				});

			connectorHelper = new WorkerConnectorHelper(Services.Mock<IWorkerConnector>());

			executionServicesMock = Services.Mock<IExecutionServices>();
			executionServicesMock.Setup(s => s.GetGameModule(It.IsAny<string>())).Returns(ExternalGameModule);
			executionServicesMock.Setup(s => s.GetWorkingDirectory(It.IsAny<WorkMessageBase>()))
				.Returns(NewDirectory);
			executionServicesMock
				.Setup(s => s.DownloadSubmission(It.IsAny<string>(), It.IsAny<string>()))
				.Callback((string _, string l) => File.WriteAllText(Path.Combine(l, "a"), "random content"));

			var reg = Services.Mock<IGameModuleRegistry>();
			reg.Setup(r => r.GetAllModules()).Returns(new[] {ExternalGameModule});
		}

		[Theory]
		[InlineData(0, 0, 0)]
		[InlineData(Constants.GameModuleNegativeExitCode, 0, 0)]
		[InlineData(0, Constants.GameModuleNegativeExitCode, 0)]
		[InlineData(0, 0, Constants.GameModuleNegativeExitCode)]
		public void Test(int check, int compile, int validate)
		{
			GameModuleConfiguration.Checker =
				CreateEntryPoint(() => EntryPoints.ExitWithCode(check, null, null));
			GameModuleConfiguration.Compiler =
				CreateEntryPoint(() => EntryPoints.ExitWithCode(compile, null, null, null));
			GameModuleConfiguration.Validator =
				CreateEntryPoint(() => EntryPoints.ExitWithCode(validate, null, null));

			connectorHelper.SetupConsumerReceive(new SubmissionValidationRequest { Game = "", Id = Guid.NewGuid() });

			GetService<Worker>().Run();

			connectorHelper.Mock.Verify(c => c.SendMessage(It.Is<SubmissionValidationResult>(m => m.JobStatus == JobStatus.Ok)));
		}
	}
}