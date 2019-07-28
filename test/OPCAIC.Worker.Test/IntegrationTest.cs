﻿using System;
using System.IO;
using System.Threading;
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
		private readonly Mock<IDownloadService> downloadServiceMock;
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
			executionServicesMock
				.Setup(s => s.GetWorkingDirectory(It.IsAny<WorkMessageBase>()))
				.Returns(NewDirectory);

			downloadServiceMock = Services.Mock<IDownloadService>();
			downloadServiceMock
				.Setup(s => s.DownloadSubmission(It.IsAny<long>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
				.Callback(MockExtensions.WriteRandomContentToTargetFolder);

			var reg = Services.Mock<IGameModuleRegistry>();
			reg.Setup(r => r.FindGameModule(It.IsAny<string>())).Returns(ExternalGameModule);
			reg.Setup(r => r.GetAllModules()).Returns(new[] { ExternalGameModule });
		}

		[Theory]
		[InlineData(0, 0, 0)]
		[InlineData(Constants.GameModuleNegativeExitCode, 0, 0)]
		[InlineData(0, Constants.GameModuleNegativeExitCode, 0)]
		[InlineData(0, 0, Constants.GameModuleNegativeExitCode)]
		public void Test(int check, int compile, int validate)
		{
			GameModuleConfiguration.Checker = ExternalGameModuleHelper.CreateEntryPoint(
				() => EntryPoints.ExitWithCode(check, null, null, null));
			GameModuleConfiguration.Compiler = ExternalGameModuleHelper.CreateEntryPoint(
				() => EntryPoints.ExitWithCode(compile, null, null, null, null));
			GameModuleConfiguration.Validator = ExternalGameModuleHelper.CreateEntryPoint(
				() => EntryPoints.ExitWithCode(validate, null, null, null));

			connectorHelper.SetupConsumerReceive(new SubmissionValidationRequest { Game = "", Id = Guid.NewGuid() });

			GetService<Worker>().Run();

			connectorHelper.Mock.Verify(c => c.SendMessage(It.Is<SubmissionValidationResult>(m => m.JobStatus == JobStatus.Ok)));
		}
	}
}