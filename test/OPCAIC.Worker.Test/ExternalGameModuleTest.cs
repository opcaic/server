using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using GameModuleMock;
using Microsoft.Extensions.DependencyInjection;
using OPCAIC.GameModules.Interface;
using OPCAIC.TestUtils;
using OPCAIC.Worker.GameModules;
using Xunit;
using Xunit.Abstractions;
using Microsoft.Extensions.Logging;

namespace OPCAIC.Worker.Test
{
	public class ExternalGameModuleTest : ServiceTestBase
	{
		protected EntryPointConfiguration config;

		/// <inheritdoc />
		protected ExternalGameModuleTest(ITestOutputHelper output) : base(output)
		{
			GameModuleConfiguration = new ExternalGameModuleConfiguration();

			externalGameModuleLazy = GetLazyService<ExternalGameModule>();
			Services.AddTransient(sp 
					=> new ExternalGameModule(sp.GetRequiredService<ILogger<ExternalGameModule>>(),
						GameModuleConfiguration,
						"GameModuleMock",
						Directory.GetCurrentDirectory()));
			Bot = new BotInfo {SourceDirectory = NewDirectory(), BinaryDirectory = NewDirectory()};
			OutputDir = NewDirectory();
			config = new EntryPointConfiguration {AdditionalFiles = NewDirectory()};
		}

		protected ExternalGameModuleConfiguration GameModuleConfiguration { get; }

		private readonly Lazy<ExternalGameModule> externalGameModuleLazy;
		protected ExternalGameModule ExternalGameModule => externalGameModuleLazy.Value;

		protected BotInfo Bot { get; }

		public DirectoryInfo OutputDir { get; }

		private void AssertLogsExist(string prefix)
		{
			AssertEx.FileExists(Path.Combine(OutputDir.FullName,
				$"{prefix}{Constants.FileNames.StdoutLogSuffix}"));
			AssertEx.FileExists(Path.Combine(OutputDir.FullName,
				$"{prefix}{Constants.FileNames.StderrLogSuffix}"));
		}

		public class SimpleTests : ExternalGameModuleTest
		{
			/// <inheritdoc />
			public SimpleTests(ITestOutputHelper output) : base(output)
			{
			}

			[Theory]
			[InlineData(0, GameModuleEntryPointResult.Success)]
			[InlineData(Constants.GameModuleNegativeExitCode, GameModuleEntryPointResult.Failure)]
			[InlineData(10, GameModuleEntryPointResult.ModuleError)]
			[InlineData(255, GameModuleEntryPointResult.ModuleError)]
			public async Task InvokeProcessSimple(int exitCode, GameModuleEntryPointResult expected)
			{
				var actual = await ExternalGameModule.RunProcessAsync(
					ExternalGameModuleHelper.CreateArgs(() => EntryPoints.ExitWithCode(exitCode)));

				Assert.Equal(expected, actual);
			}


			[Fact]
			public async Task SimpleEntryPoint()
			{
				GameModuleConfiguration.Compiler =
					ExternalGameModuleHelper.CreateEntryPoint(()
						=> EntryPoints.EchoArgs(null, null, null));

				var result = await ExternalGameModule.Compile(config, Bot, OutputDir,
					CancellationToken.None);

				Assert.Equal(GameModuleEntryPointResult.Success, result.EntryPointResult);

				var stdout = File.ReadAllText(Path.Combine(OutputDir.FullName,
					$"{Constants.FileNames.CompilerPrefix}.{Bot.Index}{Constants.FileNames.StdoutLogSuffix}"));
				var stderr = File.ReadAllText(Path.Combine(OutputDir.FullName,
					$"{Constants.FileNames.CompilerPrefix}.{Bot.Index}{Constants.FileNames.StderrLogSuffix}"));

				Assert.Contains(Bot.SourceDirectory.FullName, stdout);
				Assert.Contains(Bot.BinaryDirectory.FullName, stderr);
			}

			[Fact(Timeout = 1000)]
			public async Task TerminateProcessWhenCanceled()
			{
				var cts = new CancellationTokenSource();
				var task = Assert.ThrowsAsync<TaskCanceledException>(()
					=> ExternalGameModule.RunProcessAsync(
						ExternalGameModuleHelper.CreateArgs(() => EntryPoints.WaitIndefinitely()),
						cts.Token));
				// small delay to make sure the process is really started
				await Task.Delay(50);
				cts.Cancel();
				await task;
			}
		}

		public class ExecutorEntryPoint : ExternalGameModuleTest
		{
			/// <inheritdoc />
			public ExecutorEntryPoint(ITestOutputHelper output) : base(output)
			{
			}

			[Theory]
			[InlineData(0)]
			[InlineData(2)]
			public async Task MismatchedPlayerCount(int actualCount)
			{
				GameModuleConfiguration.Executor = ExternalGameModuleHelper.CreateEntryPoint(()
					=> EntryPoints.SingleplayerExecute(actualCount, null, null, null));

				await Assert.ThrowsAsync<MalformedMatchResultException>(()
					=> ExternalGameModule.Execute(config, Enumerable.Repeat(Bot, 1),
						OutputDir, CancellationToken.None));

				AssertLogsExist(Constants.FileNames.ExecutorPrefix);
			}

			[Fact]
			public async Task MissingResult()
			{
				GameModuleConfiguration.Executor =
					ExternalGameModuleHelper.CreateEntryPoint(()
						=> EntryPoints.EchoArgs(null, null, null));

				await Assert.ThrowsAsync<GameModuleException>(()
					=> ExternalGameModule.Execute(config, Enumerable.Repeat(Bot, 1),
						OutputDir, CancellationToken.None));

				AssertLogsExist(Constants.FileNames.ExecutorPrefix);
			}

			[Fact]
			public async Task SimpleExec()
			{
				GameModuleConfiguration.Executor = ExternalGameModuleHelper.CreateEntryPoint(()
					=> EntryPoints.SingleplayerExecute(1, null, null, null));

				await ExternalGameModule.Execute(config, Enumerable.Repeat(Bot, 1),
					OutputDir, CancellationToken.None);

				AssertLogsExist(Constants.FileNames.ExecutorPrefix);
			}
		}
	}
}