using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using Moq.Language.Flow;
using OPCAIC.Worker.GameModules;
using OPCAIC.Worker.Services;

namespace OPCAIC.Worker.Test
{
	public static class MockExtensions
	{
		public static ISetup<IGameModule, Task<CheckerResult>> SetupChecker(
			this Mock<IGameModule> mock)
		{
			Debug.Assert(mock != null);

			return mock.Setup(m => m.Check(
				It.IsAny<SubmissionInfo>(), It.IsAny<string>(),
				It.IsAny<CancellationToken>()));
		}

		public static IReturnsResult<IGameModule> SetupChecker(
			this Mock<IGameModule> mock, GameModuleEntryPointResult result)
			=> mock.SetupChecker()
				.ReturnsAsync(new CheckerResult {EntryPointResult = result});

		public static ISetup<IGameModule, Task<CompilerResult>> SetupCompiler(
			this Mock<IGameModule> mock)
		{
			Debug.Assert(mock != null);

			return mock.Setup(m => m.Compile(
				It.IsAny<SubmissionInfo>(), It.IsAny<string>(),
				It.IsAny<CancellationToken>()));
		}

		public static IReturnsResult<IGameModule> SetupCompiler(
			this Mock<IGameModule> mock, GameModuleEntryPointResult result)
			=> mock.SetupCompiler()
				.ReturnsAsync(new CompilerResult {EntryPointResult = result});

		public static ISetup<IGameModule, Task<ValidatorResult>> SetupValidator(
			this Mock<IGameModule> mock)
		{
			Debug.Assert(mock != null);

			return mock.Setup(m => m.Validate(
				It.IsAny<SubmissionInfo>(), It.IsAny<string>(),
				It.IsAny<CancellationToken>()));
		}

		public static IReturnsResult<IGameModule> SetupValidator(
			this Mock<IGameModule> mock, GameModuleEntryPointResult result)
			=> mock.SetupValidator()
				.ReturnsAsync(new ValidatorResult {EntryPointResult = result});
	}
}