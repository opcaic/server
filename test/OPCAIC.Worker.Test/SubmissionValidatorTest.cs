using System;
using System.Threading.Tasks;
using OPCAIC.GameModules.Interface;
using OPCAIC.Messaging.Messages;
using OPCAIC.Worker.Services;
using Xunit;
using Xunit.Abstractions;

namespace OPCAIC.Worker.Test
{
	public class SubmissionValidatorTest : JobExecutorTest
	{
		/// <inheritdoc />
		public SubmissionValidatorTest(ITestOutputHelper output) : base(output)
		{
			Request = new SubmissionValidationRequest
			{
				Game = "MockGame", Id = Guid.NewGuid(), SubmissionId = 42
			};
		}

		private SubmissionValidationRequest Request { get; }

		private SubmissionValidator SubmissionValidator => GetService<SubmissionValidator>();

		[Theory]
		[InlineData(1)]
		[InlineData(2)]
		[InlineData(3)]
		public async Task ExecutesUntilUserError(int successfulStages)
		{
			var i = 0;

			GameModuleEntryPointResult ResultFactory()
			{
				return i == successfulStages
					? GameModuleEntryPointResult.Failure
					: GameModuleEntryPointResult.Success;
			}

			if (i++ < successfulStages)
			{
				GameModuleMock.SetupChecker().Returns(ResultFactory());
			}

			if (i++ < successfulStages)
			{
				GameModuleMock.SetupCompiler().Returns(ResultFactory());
			}

			if (i++ < successfulStages)
			{
				GameModuleMock.SetupValidator().Returns(ResultFactory());
			}

			var result = await SubmissionValidator.ExecuteAsync(Request);
			Assert.Equal(JobStatus.Ok, result.JobStatus); // still should return Ok

			i = 0;

			SubTaskResult ResultSelector()
			{
				return ++i < successfulStages ? SubTaskResult.Ok :
					i == successfulStages ? SubTaskResult.NotOk : SubTaskResult.Unknown;
			}

			Assert.Equal(ResultSelector(), result.CheckerResult);
			Assert.Equal(ResultSelector(), result.CompilerResult);
			Assert.Equal(ResultSelector(), result.ValidatorResult);
		}

		/// <inheritdoc />
		protected override void Dispose(bool disposing)
		{
			GameModuleMock.VerifyAll();
			ExecutionServicesMock.VerifyAll();

			base.Dispose(disposing);
		}

		[Fact]
		public async Task ExecutesMatchSuccessfully()
		{
			GameModuleMock.SetupChecker().Returns(GameModuleEntryPointResult.Success);
			GameModuleMock.SetupCompiler().Returns(GameModuleEntryPointResult.Success);
			GameModuleMock.SetupValidator().Returns(GameModuleEntryPointResult.Success);

			var result = await SubmissionValidator.ExecuteAsync(Request);
			Assert.Equal(JobStatus.Ok, result.JobStatus);
		}

		[Fact]
		public async Task ReportsModuleError()
		{
			GameModuleMock.SetupChecker().Returns(GameModuleEntryPointResult.ModuleError);

			var result = await SubmissionValidator.ExecuteAsync(Request);

			Assert.Equal(JobStatus.Error, result.JobStatus);
			Assert.Equal(SubTaskResult.ModuleError, result.CheckerResult);
			Assert.Equal(SubTaskResult.Unknown, result.CompilerResult);
			Assert.Equal(SubTaskResult.Unknown, result.ValidatorResult);
		}

		[Fact]
		public async Task ReportsModuleErrorException()
		{
			GameModuleMock.SetupChecker().Throws<GameModuleException>();

			var result = await SubmissionValidator.ExecuteAsync(Request);

			Assert.Equal(JobStatus.Error, result.JobStatus);
			Assert.Equal(SubTaskResult.ModuleError, result.CheckerResult);
			Assert.Equal(SubTaskResult.Unknown, result.CompilerResult);
			Assert.Equal(SubTaskResult.Unknown, result.ValidatorResult);
		}

		[Fact]
		public async Task ReportsPlatformError()
		{
			GameModuleMock.SetupChecker().Throws<Exception>();

			var result = await SubmissionValidator.ExecuteAsync(Request);

			Assert.Equal(JobStatus.Error, result.JobStatus);
			Assert.Equal(SubTaskResult.PlatformError, result.CheckerResult);
			Assert.Equal(SubTaskResult.Unknown, result.CompilerResult);
			Assert.Equal(SubTaskResult.Unknown, result.ValidatorResult);
		}

		[Fact]
		public async Task ThrowsOnTaskCancelled()
		{
			GameModuleMock.SetupChecker().Throws<OperationCanceledException>();

			var result = await SubmissionValidator.ExecuteAsync(Request);

			Assert.Equal(JobStatus.Timeout, result.JobStatus);
			Assert.Equal(SubTaskResult.Aborted, result.CheckerResult);
			Assert.Equal(SubTaskResult.Unknown, result.CompilerResult);
			Assert.Equal(SubTaskResult.Unknown, result.ValidatorResult);
		}
	}
}