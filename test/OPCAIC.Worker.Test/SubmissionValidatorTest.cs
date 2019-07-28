using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using OPCAIC.Messaging.Messages;
using OPCAIC.TestUtils;
using OPCAIC.Worker.Config;
using OPCAIC.Worker.GameModules;
using OPCAIC.Worker.Services;
using Xunit;
using Xunit.Abstractions;

namespace OPCAIC.Worker.Test
{
	public class SubmissionValidatorTest : ServiceTestBase
	{
		private readonly Mock<IGameModule> gameModuleMock;
		private readonly Mock<IDownloadService> downloadServiceMock;
		private readonly Mock<IExecutionServices> executionServicesMock;
		private DirectoryInfo ArchiveDirectory { get; }
		private DirectoryInfo WorkingDirectory { get; }

		private SubmissionValidationRequest Request { get; }
		/// <inheritdoc />
		public SubmissionValidatorTest(ITestOutputHelper output) : base(output)
		{
			ArchiveDirectory = NewDirectory();
			WorkingDirectory = NewDirectory();

			Services.Configure<ExecutionConfig>(cfg =>
			{
				cfg.ArchiveDirectoryRoot = ArchiveDirectory.FullName;
				cfg.WorkingDirectoryRoot = WorkingDirectory.FullName;
				cfg.MaxTaskTimeout = 10000;
			});

			gameModuleMock = Services.Mock<IGameModule>();
			executionServicesMock = Services.Mock<IExecutionServices>();

			executionServicesMock
				.Setup(s => s.GetGameModule(It.IsAny<string>()))
				.Returns(gameModuleMock.Object);

			executionServicesMock
				.Setup(s => s.GetWorkingDirectory(It.IsAny<WorkMessageBase>()))
				.Returns(NewDirectory);

			downloadServiceMock = Services.Mock<IDownloadService>();
			downloadServiceMock
				.Setup(s => s.DownloadSubmission(It.IsAny<long>(), It.IsAny<string>()))
				// just some nonempty submission
				.Callback((long _, string l) => File.WriteAllText(Path.Combine(l, "a"), "random content"));

			Request = new SubmissionValidationRequest
			{
				Game = "MockGame",
				Id = Guid.NewGuid(),
				SubmissionId = 42
			};
		}

		private SubmissionValidator SubmissionValidator => GetService<SubmissionValidator>();

		[Fact]
		public async Task ExecutesMatchSuccessfully()
		{
			gameModuleMock.SetupChecker(GameModuleEntryPointResult.Success);
			gameModuleMock.SetupCompiler(GameModuleEntryPointResult.Success);
			gameModuleMock.SetupValidator(GameModuleEntryPointResult.Success);

			var result = await SubmissionValidator.ExecuteAsync(Request);
			Assert.Equal(JobStatus.Ok, result.JobStatus);
		}

		[Theory]
		[InlineData(1)]
		[InlineData(2)]
		[InlineData(3)]
		public async Task ExecutesUntilUserError(int successfulStages)
		{
			int i = 0;
			GameModuleEntryPointResult ResultFactory() => i == successfulStages ? GameModuleEntryPointResult.Failure: GameModuleEntryPointResult.Success;

			if (i++ < successfulStages) gameModuleMock.SetupChecker(ResultFactory());
			if (i++ < successfulStages) gameModuleMock.SetupCompiler(ResultFactory());
			if (i++ < successfulStages) gameModuleMock.SetupValidator(ResultFactory());

			var result = await SubmissionValidator.ExecuteAsync(Request);
			Assert.Equal(JobStatus.Ok, result.JobStatus); // still should return Ok

			i = 0;

			SubTaskResult ResultSelector()
				=> ++i < successfulStages ? SubTaskResult.Ok :
					i == successfulStages ? SubTaskResult.NotOk : SubTaskResult.Unknown;

			Assert.Equal(ResultSelector(), result.CheckerResult); 
			Assert.Equal(ResultSelector(), result.CompilerResult); 
			Assert.Equal(ResultSelector(), result.ValidatorResult); 
		}

		[Fact]
		public async Task ThrowsOnTaskCancelled()
		{
			gameModuleMock.SetupChecker().Throws<OperationCanceledException>();

			var result = await SubmissionValidator.ExecuteAsync(Request);

			Assert.Equal(JobStatus.Timeout, result.JobStatus); 
			Assert.Equal(SubTaskResult.Aborted, result.CheckerResult);
			Assert.Equal(SubTaskResult.Unknown, result.CompilerResult);
			Assert.Equal(SubTaskResult.Unknown, result.ValidatorResult);
		}

		[Fact]
		public async Task ReportsModuleErrorException()
		{
			gameModuleMock.SetupChecker().Throws<GameModuleException>();

			var result = await SubmissionValidator.ExecuteAsync(Request);

			Assert.Equal(JobStatus.Error, result.JobStatus); 
			Assert.Equal(SubTaskResult.ModuleError, result.CheckerResult);
			Assert.Equal(SubTaskResult.Unknown, result.CompilerResult);
			Assert.Equal(SubTaskResult.Unknown, result.ValidatorResult);
		}

		[Fact]
		public async Task ReportsModuleError()
		{
			gameModuleMock.SetupChecker(GameModuleEntryPointResult.ModuleError);

			var result = await SubmissionValidator.ExecuteAsync(Request);

			Assert.Equal(JobStatus.Error, result.JobStatus); 
			Assert.Equal(SubTaskResult.ModuleError, result.CheckerResult);
			Assert.Equal(SubTaskResult.Unknown, result.CompilerResult);
			Assert.Equal(SubTaskResult.Unknown, result.ValidatorResult);
		}

		[Fact]
		public async Task ReportsPlatformError()
		{
			gameModuleMock.SetupChecker().Throws<Exception>();

			var result = await SubmissionValidator.ExecuteAsync(Request);

			Assert.Equal(JobStatus.Error, result.JobStatus); 
			Assert.Equal(SubTaskResult.PlatformError, result.CheckerResult);
			Assert.Equal(SubTaskResult.Unknown, result.CompilerResult);
			Assert.Equal(SubTaskResult.Unknown, result.ValidatorResult);
		}

		/// <inheritdoc />
		protected override void Dispose(bool disposing)
		{
			gameModuleMock.VerifyAll();
			executionServicesMock.VerifyAll();

			base.Dispose(disposing);
		}
	}
}