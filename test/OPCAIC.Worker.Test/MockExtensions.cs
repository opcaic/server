using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using Moq.Language;
using OPCAIC.GameModules.Interface;

namespace OPCAIC.Worker.Test
{
	public static class MockExtensions
	{
		public static readonly Action<long, string, CancellationToken>
			WriteRandomContentToTargetFolder = (_, path, __) =>
			{
				File.WriteAllText(Path.Combine(path, "a"), "random content");
			};

		public static ISetupSequentialResult<Task<CheckerResult>> SetupChecker(
			this Mock<IGameModule> mock)
		{
			Debug.Assert(mock != null);

			return mock.SetupSequence(m => m.Check(It.IsAny<EntryPointConfiguration>(),
				It.IsAny<BotInfo>(), It.IsAny<DirectoryInfo>(),
				It.IsAny<CancellationToken>()));
		}

		public static ISetupSequentialResult<Task<T>> Returns<T>(
			this ISetupSequentialResult<Task<T>> setup, GameModuleEntryPointResult result)
			where T : GameModuleResult, new()
			=> setup.ReturnsAsync(new T {EntryPointResult = result});

		public static ISetupSequentialResult<Task<CompilerResult>> SetupCompiler(
			this Mock<IGameModule> mock)
		{
			Debug.Assert(mock != null);

			return mock.SetupSequence(m => m.Compile(It.IsAny<EntryPointConfiguration>(),
				It.IsAny<BotInfo>(), It.IsAny<DirectoryInfo>(),
				It.IsAny<CancellationToken>()));
		}

		public static ISetupSequentialResult<Task<ValidatorResult>> SetupValidator(
			this Mock<IGameModule> mock)
		{
			Debug.Assert(mock != null);

			return mock.SetupSequence(m => m.Validate(It.IsAny<EntryPointConfiguration>(),
				It.IsAny<BotInfo>(), It.IsAny<DirectoryInfo>(),
				It.IsAny<CancellationToken>()));
		}

		public static ISetupSequentialResult<Task<ExecutorResult>> SetupExecutor(
			this Mock<IGameModule> mock)
		{
			Debug.Assert(mock != null);

			return mock.SetupSequence(m => m.Execute(It.IsAny<EntryPointConfiguration>(),
				It.IsAny<IEnumerable<BotInfo>>(), It.IsAny<DirectoryInfo>(),
				It.IsAny<CancellationToken>()));
		}

		public static ISetupSequentialResult<Task<ExecutorResult>> SetupExecutor(
			this Mock<IGameModule> mock, GameModuleEntryPointResult result,
			MatchResult matchResult = null)
			=> mock.SetupExecutor()
				.ReturnsAsync(new ExecutorResult
				{
					EntryPointResult = result, MatchResult = matchResult
				});
	}
}