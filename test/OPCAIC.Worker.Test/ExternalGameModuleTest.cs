using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using GameModuleMock;
using OPCAIC.TestUtils;
using OPCAIC.Worker.GameModules;
using OPCAIC.Worker.Services;
using Xunit;
using Xunit.Abstractions;

namespace OPCAIC.Worker.Test
{
	public class ExternalGameModuleTest : ServiceTestBase
	{
		/// <inheritdoc />
		protected ExternalGameModuleTest(ITestOutputHelper output) : base(output)
		{
			var logger = new XUnitLogger<ExternalGameModule>(output);
			GameModuleConfiguration = new GameModuleConfiguration();
			ExternalGameModule = new ExternalGameModule(logger, GameModuleConfiguration,
				"GameModuleMock",
				Directory.GetCurrentDirectory());
			Submission = new SubmissionInfo
			{
				SourceDirectory = NewDirectory(),
				BinaryDirectory = NewDirectory()
			};
			OutputDir = NewDirectory();
		}

		protected GameModuleConfiguration GameModuleConfiguration { get; }

		protected ExternalGameModule ExternalGameModule { get; }

		protected SubmissionInfo Submission { get; }

		public DirectoryInfo OutputDir { get; }

		protected static EntryPointConfiguration CreateEntryPoint(Expression<Func<int>> invocation)
			=> new EntryPointConfiguration
			{ // this should be platform independent :)
				Executable = "dotnet",
				Arguments = new[] {$"{nameof(GameModuleMock)}.dll"}
					.Concat(GetCmdLineArguments(invocation)).ToArray()
			};

		private static GameModuleProcessArgs CreateArgs(Expression<Func<int>> invocation)
			=> new GameModuleProcessArgs
			{
				WorkingDirectory = Directory.GetCurrentDirectory(),
				EntryPoint = CreateEntryPoint(invocation)
			};

		private static List<string> GetCmdLineArguments(Expression<Func<int>> invocation)
		{
			try
			{
				var body = (MethodCallExpression)invocation.Body;
				var arguments = new List<string> {body.Method.Name};
				var wasNull = false;
				foreach (var arg in body.Arguments)
				{
					object value;
					switch (arg)
					{
						case ConstantExpression ce: // inline constant
							value = ce.Value;
							break;

						default: // we expect a captured variable, nothing else is supported
							var me = (MemberExpression)arg;
							value = ((FieldInfo)me.Member)
								.GetValue(((ConstantExpression)me.Expression).Value);
							break;
					}

					// allow nulls as placeholders for real entry point arguments
					if (value != null)
					{
						Assert.False(wasNull,
							"Nulls (placeholders) cannot be followed by a non-null value.");
						arguments.Add(value.ToString());
					}
					else
					{
						wasNull = true;
					}
				}

				return arguments;
			}
			catch
			{
				throw new NotSupportedException(
					$"The invocation expression must be in the form of () => {nameof(EntryPoints)}.[Method]([args]), where args are either constants or captured variables. The given values must not be null with the exception of placeholders for the actual entry point arguments inserted by the game module.");
			}
		}

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
					CreateArgs(() => EntryPoints.ExitWithCode(exitCode)));

				Assert.Equal(expected, actual);
			}


			[Fact]
			public async Task SimpleEntryPoint()
			{
				GameModuleConfiguration.Checker =
					CreateEntryPoint(() => EntryPoints.EchoArgs(null, null));

				var result = await ExternalGameModule.Check(Submission, OutputDir.FullName,
					CancellationToken.None);

				Assert.Equal(GameModuleEntryPointResult.Success, result.EntryPointResult);

				var stdout = File.ReadAllText(Path.Combine(OutputDir.FullName,
					$"{Constants.FileNames.CheckerPrefix}.{Submission.Index}{Constants.FileNames.StdoutLogSuffix}"));
				var stderr = File.ReadAllText(Path.Combine(OutputDir.FullName,
					$"{Constants.FileNames.CheckerPrefix}.{Submission.Index}{Constants.FileNames.StderrLogSuffix}"));

				Assert.Contains(Submission.SourceDirectory.FullName, stdout);
				Assert.Contains(OutputDir.FullName, stderr);
			}

			[Fact(Timeout = 1000)]
			public async Task TerminateProcessWhenCanceled()
			{
				var cts = new CancellationTokenSource();
				var task = Assert.ThrowsAsync<TaskCanceledException>(()
					=> ExternalGameModule.RunProcessAsync(
						CreateArgs(() => EntryPoints.WaitIndefinitely()),
						cts.Token));

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
				GameModuleConfiguration.Executor =
					CreateEntryPoint(()
						=> EntryPoints.SingleplayerExecute(actualCount, null, null));

				await Assert.ThrowsAsync<MalformedMatchResultException>(()
					=> ExternalGameModule.Execute(Enumerable.Repeat(Submission, 1),
						OutputDir.FullName, CancellationToken.None));

				AssertLogsExist(Constants.FileNames.ExecutorPrefix);
			}

			[Fact]
			public async Task MissingResult()
			{
				GameModuleConfiguration.Executor =
					CreateEntryPoint(() => EntryPoints.EchoArgs(null, null));

				await Assert.ThrowsAsync<GameModuleException>(()
					=> ExternalGameModule.Execute(Enumerable.Repeat(Submission, 1),
						OutputDir.FullName, CancellationToken.None));

				AssertLogsExist(Constants.FileNames.ExecutorPrefix);
			}

			[Fact]
			public async Task SimpleExec()
			{
				GameModuleConfiguration.Executor =
					CreateEntryPoint(() => EntryPoints.SingleplayerExecute(1, null, null));

				await ExternalGameModule.Execute(Enumerable.Repeat(Submission, 1),
					OutputDir.FullName, CancellationToken.None);

				AssertLogsExist(Constants.FileNames.ExecutorPrefix);
			}
		}
	}
}