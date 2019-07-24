using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using OPCAIC.Utils;
using OPCAIC.Worker.Services;

namespace OPCAIC.Worker.GameModules
{
	/// <summary>
	///     Represents an external game module, executed as an external script.
	/// </summary>
	public class ExternalGameModule : IGameModule
	{
		private readonly GameModuleConfiguration config;
		private readonly ILogger logger;
		private readonly string rootDir;

		/// <summary>
		///     Creates a new external game module object.
		/// </summary>
		/// <param name="logger">Logger of the game module.</param>
		/// <param name="config">Configuration listing the entry point names.</param>
		/// <param name="gameName">Name of the game represented by the module.</param>
		/// <param name="rootDir">Root (working) directory of the game.</param>
		public ExternalGameModule(ILogger logger, GameModuleConfiguration config, string gameName,
			string rootDir)
		{
			this.logger = logger;
			this.rootDir = rootDir;
			GameName = gameName;
			this.config = config;
		}

		/// <inheritdoc />
		public string GameName { get; }

		/// <inheritdoc />
		public async Task<CheckerResult> Check(SubmissionInfo submission, string outputDir,
			CancellationToken cancellationToken)
		{
			Require.ArgNotNull(submission, nameof(submission));
			Require.ArgNotNull(outputDir, nameof(outputDir));

			var logPrefix = Path.Combine(outputDir,
				$"{Constants.FileNames.CheckerPrefix}.{submission.Index}");
			var procStart = new GameModuleProcessArgs
			{
				WorkingDirectory = rootDir,
				EntryPoint = config.Checker,
				Arguments = {submission.SourceDirectory.FullName, outputDir}
			};

			return await InvokeGameModule<CheckerResult>(procStart, logPrefix, cancellationToken);
		}

		/// <inheritdoc />
		public async Task<CompilerResult> Compile(SubmissionInfo submission, string outputDir,
			CancellationToken cancellationToken)
		{
			Require.ArgNotNull(submission, nameof(submission));
			Require.ArgNotNull(outputDir, nameof(outputDir));

			var logPrefix = Path.Combine(outputDir,
				$"{Constants.FileNames.CompilerPrefix}.{submission.Index}");
			var procStart = new GameModuleProcessArgs
			{
				WorkingDirectory = rootDir,
				EntryPoint = config.Compiler,
				Arguments =
				{
					submission.SourceDirectory.FullName,
					submission.BinaryDirectory.FullName,
					outputDir
				}
			};

			return await InvokeGameModule<CompilerResult>(procStart, logPrefix, cancellationToken);
		}

		/// <inheritdoc />
		public async Task<ValidatorResult> Validate(SubmissionInfo submission, string outputDir,
			CancellationToken cancellationToken)
		{
			Require.ArgNotNull(submission, nameof(submission));
			Require.ArgNotNull(outputDir, nameof(outputDir));

			var logPrefix = Path.Combine(outputDir,
				$"{Constants.FileNames.ValidatorPrefix}.{submission.Index}");
			var procStart = new GameModuleProcessArgs
			{
				WorkingDirectory = rootDir,
				EntryPoint = config.Validator,
				Arguments = {submission.BinaryDirectory.FullName, outputDir}
			};

			return await InvokeGameModule<ValidatorResult>(procStart, logPrefix, cancellationToken);
		}

		/// <inheritdoc />
		public async Task<ExecutorResult> Execute(IEnumerable<SubmissionInfo> submissions,
			string outputDir,
			CancellationToken cancellationToken)
		{
			Require.NotEmpty<ArgumentException>(submissions, nameof(submissions));
			Require.ArgNotNull(outputDir, nameof(outputDir));

			var logPrefix = Path.Combine(outputDir, Constants.FileNames.ExecutorPrefix);
			var procStart = new GameModuleProcessArgs
			{
				WorkingDirectory = rootDir,
				EntryPoint = config.Executor,
				// 1..N participating bots
				Arguments = submissions.Select(s => s.BinaryDirectory.FullName).ToList()
			};
			procStart.Arguments.Add(outputDir);

			var entryPointRes =
				await InvokeGameModule<ExecutorResult>(procStart, logPrefix, cancellationToken);

			if (entryPointRes.EntryPointResult == GameModuleEntryPointResult.Success)
			{
				logger.LogInformation("Reading results file");
				entryPointRes.MatchResult = ReadMatchResult(
					Path.Combine(outputDir, Constants.FileNames.ExecutorResult),
					procStart.Arguments.Count - 1);
			}

			return entryPointRes;
		}

		/// <summary>
		///     Reads and validates an instance of <see cref="MatchResult"/> from the file in the given path.
		/// </summary>
		/// <param name="path">Path to the result file.</param>
		/// <param name="expectedResultCount">Expected number of results for validation.</param>
		/// <returns></returns>
		private static MatchResult ReadMatchResult(string path, int expectedResultCount)
		{
			MatchResult res;
			try
			{
				res = JsonHelper.DeserializeWithExtra<MatchResult>(
					File.ReadAllText(path));
			}
			catch (Exception e)
			{
				throw new GameModuleException("Failed to read match results.", e);
			}

			if (res.Results.Length != expectedResultCount)
			{
				throw new MalformedMatchResultException("Number of results does not match the number of players.");
			}

			return res;
		}

		/// <inheritdoc />
		public Task Clean(CancellationToken cancellationToken) => Task.CompletedTask;

		/// <summary>
		///     Invokes game module entry point and returns its result.
		/// </summary>
		/// <param name="args">Arguments of the entry point.</param>
		/// <param name="logPrefix">Prefix of the path where logs should be stored.</param>
		/// <param name="cancellationToken">Cancellation token used to cancel the request prematurely.</param>
		/// <returns>Complete or incomplete result of the game module, depending on the result of the entry point process.</returns>
		private async Task<TResult> InvokeGameModule<TResult>(GameModuleProcessArgs args,
			string logPrefix,
			CancellationToken cancellationToken)
			where TResult : GameModuleResult, new()
		{
			using (var stdout =
				new StreamWriter($"{logPrefix}{Constants.FileNames.StdoutLogSuffix}"))
			using (var stderr =
				new StreamWriter($"{logPrefix}{Constants.FileNames.StderrLogSuffix}"))
			{
				args.StandardOutput = stdout;
				args.StandardError = stderr;

				var result = new TResult();
				var exitCode = await RunProcessAsync(args, cancellationToken);

				switch (exitCode)
				{
					case GameModuleEntryPointResult.Success:
						result.EntryPointResult = GameModuleEntryPointResult.Success;
						break;

					case GameModuleEntryPointResult.Failure:
						result.EntryPointResult = GameModuleEntryPointResult.Failure;
						break;

					case GameModuleEntryPointResult.ModuleError:
						result.EntryPointResult = GameModuleEntryPointResult.ModuleError;
						break;

					default:
						throw new InvalidOperationException(
							$"Invalid {nameof(GameModuleEntryPointResult)}: {exitCode}.");
				}

				logger.LogInformation("Entry point  finished with '{processResult}'", exitCode);

				return result;
			}
		}

		/// <summary>
		///     Executes a file in an asynchronous process.
		/// </summary>
		/// <param name="args">Set of parameters to the process to be run.</param>
		/// <param name="cancellationToken">Cancellation token used to cancel the request prematurely.</param>
		/// <returns>Task with process' resulting exitcode.</returns>
		internal async Task<GameModuleEntryPointResult> RunProcessAsync(
			GameModuleProcessArgs args,
			CancellationToken cancellationToken = new CancellationToken())
		{
			using (var process = new Process
			{
				StartInfo =
				{
					FileName = args.EntryPoint.Executable,
					UseShellExecute = false,
					CreateNoWindow = false,
					RedirectStandardOutput = true,
					RedirectStandardError = true,
					WorkingDirectory = args.WorkingDirectory
				},
				EnableRaisingEvents = true
			})
			{
				// push all arguments
				foreach (var arg in args.EntryPoint.Arguments.Concat(args.Arguments))
				{
					process.StartInfo.ArgumentList.Add(arg);
				}

				logger.LogInformation(
					$"Invoking {args.EntryPoint.Executable} with args '{string.Join("', '", process.StartInfo.ArgumentList)}'");

				if (!process.Start())
				{
					throw new GameModuleProcessStartException("Unable to start game module process.");
				}

				logger.LogInformation("Process started, PID: {pid}", process.Id);

				process.OutputDataReceived += (_, e) =>
				{
					args.StandardOutput.WriteLine(e.Data);
					logger.LogInformation("[stdout]: {0}", e.Data);
				};
				process.BeginOutputReadLine();

				process.ErrorDataReceived += (_, e) =>
				{
					args.StandardError.WriteLine(e.Data);
					logger.LogInformation("[stderr]: {0}", e.Data);
				};
				process.BeginErrorReadLine();

				return await WaitForExitOrKillAsync(process, cancellationToken);
			}
		}

		/// <summary>
		///     Waits for the process to exit, or terminates the process if the <paramref name="cancellationToken" /> has been
		///     canceled. Does not throw on cancellation.
		/// </summary>
		/// <param name="process">The process to be waited on</param>
		/// <param name="cancellationToken">The cancellation token.</param>
		/// <returns></returns>
		private async Task<GameModuleEntryPointResult> WaitForExitOrKillAsync(
			Process process,
			CancellationToken cancellationToken)
		{
			process.EnableRaisingEvents = true;
			var tcs = new TaskCompletionSource<GameModuleEntryPointResult>();

			void ExitHandler(object sender, EventArgs eventArgs)
			{
				logger.LogInformation("Exited with exit code {exitcode}", process.ExitCode);
				tcs.TrySetResult(ExitCodeToProcessResult(process.ExitCode));
			}

			process.Exited += ExitHandler;

			try
			{
				if (process.HasExited) // handle data race of handler registration
				{
					ExitHandler(null, null);
				}

				using (cancellationToken.Register(() =>
				{
					logger.LogWarning("Cancellation requested, killing process {pid}", process.Id);
					process.Kill();
					tcs.SetCanceled();
				}))
				{
					return await tcs.Task.ConfigureAwait(false);
				}
			}
			finally
			{
				process.Exited -= ExitHandler;
			}
		}

		/// <summary>
		///     Based on process' exit code, returns a <see cref="GameModuleEntryPointResult" />.
		/// </summary>
		/// <param name="exitCode">Exit code of the process.</param>
		/// <returns>Explicit result of the process.</returns>
		private static GameModuleEntryPointResult ExitCodeToProcessResult(int exitCode)
		{
			switch (exitCode)
			{
				case 0:
					return GameModuleEntryPointResult.Success;
				case Constants.GameModuleNegativeExitCode: 
					return GameModuleEntryPointResult.Failure;
				default:
					return GameModuleEntryPointResult.ModuleError;
			}
		}
	}
}