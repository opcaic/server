using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using OPCAIC.Common;
using OPCAIC.GameModules.Interface;
using OPCAIC.Utils;
using OPCAIC.Worker.Services;

namespace OPCAIC.Worker.GameModules
{
	/// <summary>
	///     Represents an external game module, executed as an external script.
	/// </summary>
	public class ExternalGameModule : IGameModule
	{
		private readonly ILogger logger;
		private readonly ExternalGameModuleConfiguration moduleConfig;
		private readonly string rootDir;

		/// <summary>
		///     Creates a new external game module object.
		/// </summary>
		/// <param name="logger">Logger of the game module.</param>
		/// <param name="moduleConfig">Configuration listing the entry point names.</param>
		/// <param name="gameName">Name of the game represented by the module.</param>
		/// <param name="rootDir">Root (working) directory of the game.</param>
		public ExternalGameModule(ILogger logger, ExternalGameModuleConfiguration moduleConfig,
			string gameName,
			string rootDir)
		{
			this.logger = logger;
			this.rootDir = rootDir;
			GameName = gameName;
			this.moduleConfig = moduleConfig;
		}

		/// <inheritdoc />
		public string GameName { get; }

		/// <inheritdoc />
		public Task<CheckerResult> Check(EntryPointConfiguration config, BotInfo bot,
			DirectoryInfo outputDir,
			CancellationToken cancellationToken)
		{
			Require.ArgNotNull(config, nameof(config));
			Require.ArgNotNull(bot, nameof(bot));
			Require.ArgNotNull(outputDir, nameof(outputDir));

			var logPrefix = Path.Combine(outputDir.FullName,
				$"{Constants.FileNames.CheckerPrefix}.{bot.Index}");
			var procStart = new GameModuleProcessArgs
			{
				WorkingDirectory = rootDir,
				EntryPoint = moduleConfig.Checker,
				Arguments = {config.AdditionalFiles.FullName, bot.SourceDirectory.FullName}
			};

			return ConfigureAndInvoke<CheckerResult>(procStart, logPrefix, config,
				cancellationToken);
		}

		/// <inheritdoc />
		public Task<CompilerResult> Compile(EntryPointConfiguration config, BotInfo bot,
			DirectoryInfo outputDir,
			CancellationToken cancellationToken)
		{
			Require.ArgNotNull(config, nameof(config));
			Require.ArgNotNull(bot, nameof(bot));
			Require.ArgNotNull(outputDir, nameof(outputDir));

			var logPrefix = Path.Combine(outputDir.FullName,
				$"{Constants.FileNames.CompilerPrefix}.{bot.Index}");
			var procStart = new GameModuleProcessArgs
			{
				WorkingDirectory = rootDir,
				EntryPoint = moduleConfig.Compiler,
				Arguments =
				{
					config.AdditionalFiles.FullName,
					bot.SourceDirectory.FullName,
					bot.BinaryDirectory.FullName
				}
			};

			return ConfigureAndInvoke<CompilerResult>(procStart, logPrefix, config,
				cancellationToken);
		}

		/// <inheritdoc />
		public Task<ValidatorResult> Validate(EntryPointConfiguration config, BotInfo bot,
			DirectoryInfo outputDir,
			CancellationToken cancellationToken)
		{
			Require.ArgNotNull(config, nameof(config));
			Require.ArgNotNull(bot, nameof(bot));
			Require.ArgNotNull(outputDir, nameof(outputDir));

			var logPrefix = Path.Combine(outputDir.FullName,
				$"{Constants.FileNames.ValidatorPrefix}.{bot.Index}");
			var procStart = new GameModuleProcessArgs
			{
				WorkingDirectory = rootDir,
				EntryPoint = moduleConfig.Validator,
				Arguments = {config.AdditionalFiles.FullName, bot.BinaryDirectory.FullName}
			};

			return ConfigureAndInvoke<ValidatorResult>(procStart, logPrefix, config,
				cancellationToken);
		}

		/// <inheritdoc />
		public async Task<ExecutorResult> Execute(EntryPointConfiguration config,
			IEnumerable<BotInfo> submissions,
			DirectoryInfo outputDir,
			CancellationToken cancellationToken)
		{
			Require.ArgNotNull(config, nameof(config));
			Require.ArgNotNull(submissions, nameof(submissions));
			Require.ArgNotNull(outputDir, nameof(outputDir));

			var binDirs = submissions.Select(s => s.BinaryDirectory.FullName).ToList();
			Require.NotEmpty<ArgumentException>(binDirs, nameof(submissions));

			var logPrefix = Path.Combine(outputDir.FullName, Constants.FileNames.ExecutorPrefix);
			var procStart = new GameModuleProcessArgs
			{
				WorkingDirectory = rootDir,
				EntryPoint = moduleConfig.Executor,
				Arguments = {config.AdditionalFiles.FullName}
			};
			// 1..N participating bots
			procStart.Arguments.AddRange(binDirs);
			procStart.Arguments.Add(outputDir.FullName);

			var entryPointRes =
				await ConfigureAndInvoke<ExecutorResult>(procStart, logPrefix, config,
					cancellationToken);

			if (entryPointRes.EntryPointResult == GameModuleEntryPointResult.Success)
			{
				logger.LogDebug("Reading results file");
				entryPointRes.MatchResult = ReadMatchResult(
					Path.Combine(outputDir.FullName, Constants.FileNames.ExecutorResult),
					binDirs.Count);
			}

			return entryPointRes;
		}

		/// <inheritdoc />
		public Task Clean(CancellationToken cancellationToken)
		{
			return Task.CompletedTask;
		}

		private async Task<T> ConfigureAndInvoke<T>(GameModuleProcessArgs procStart,
			string logPrefix,
			EntryPointConfiguration config, CancellationToken cancellationToken)
			where T : GameModuleResult, new()
		{
			var configPath = Path.Combine(config.AdditionalFiles.FullName,
				Constants.FileNames.GameModuleConfig);
			if (File.Exists(configPath))
			{
				logger.LogWarning(
					"file {} already exists. Contents will be overwritten.", configPath);
			}

			try
			{
				File.WriteAllText(
					configPath,
					JsonConvert.SerializeObject(config.Configuration));
				return await InvokeGameModule<T>(procStart, logPrefix, cancellationToken);
			}
			finally
			{
				File.Delete(configPath);
			}
		}

		/// <summary>
		///     Reads and validates an instance of <see cref="MatchResult" /> from the file in the given path.
		/// </summary>
		/// <param name="path">Path to the result file.</param>
		/// <param name="expectedResultCount">Expected number of results for validation.</param>
		/// <returns></returns>
		private static MatchResult ReadMatchResult(string path, int expectedResultCount)
		{
			JsonMatchResult res;
			try
			{
				res = JsonHelper.DeserializeWithExtra<JsonMatchResult>(
					File.ReadAllText(path));
			}
			catch (Exception e)
			{
				throw new GameModuleException("Failed to read match results.", e);
			}

			if (res.Results.Length != expectedResultCount)
			{
				throw new MalformedMatchResultException(
					"Number of results does not match the number of players.");
			}

			return new MatchResult
			{
				Results = res.Results.Select(r => new BotResult
				{
					Score = r.Score,
					HasCrashed = false, // TODO
					AdditionalInfo = r.AdditionalInfo
				}).ToArray(),
				AdditionalInfo = res.AdditionalInfo
			};
		}

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

				logger.LogDebug("Entry point finished with '{}'", exitCode);

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

				logger.LogDebug(
					$"Starting {args.EntryPoint.Executable} with args '{string.Join("', '", process.StartInfo.ArgumentList)}'");

				process.OutputDataReceived += (_, e) =>
				{
					args.StandardOutput.WriteLine(e.Data);
				};
				process.ErrorDataReceived += (_, e) =>
				{
					args.StandardError.WriteLine(e.Data);
				};
				
				if (!process.Start())
				{
					throw new GameModuleProcessStartException(
						"Unable to start game module process.");
				}
				process.BeginOutputReadLine();
				process.BeginErrorReadLine();

				logger.LogDebug(
					$"Process started, PID: {{{LoggingTags.GameModuleProcessId}}}", process.Id);


				var res = await WaitForExitOrKillAsync(process, cancellationToken);

				// wait for stdout and stderr buffers to flush
				process.WaitForExit();

				return res;
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

			void OnCancelled()
			{
				try
				{
					logger.LogWarning(
						$"Cancellation requested, killing process {{{LoggingTags.GameModuleProcessId}}}",
						process.Id);
					process.Kill();
					tcs.TrySetCanceled();
				}
				catch (InvalidOperationException)
				{
					// process was already finished and tcs fulfilled, ignore
				}
			}

			void ExitHandler(object sender, EventArgs eventArgs)
			{
				logger.LogInformation(
					$"Exited with exit code {{{LoggingTags.GameModuleProcessExitCode}}}",
					process.ExitCode);

				// try to set result, can fail if the task was cancelled in the meantime
				tcs.TrySetResult(ExitCodeToProcessResult(process.ExitCode));
			}

			process.Exited += ExitHandler;

			try
			{
				if (process.HasExited) // handle data race of handler registration
				{
					ExitHandler(null, null);
				}

				if (cancellationToken.IsCancellationRequested)
				{
					OnCancelled();
					return await tcs.Task;
				}
				else
				{
					using (cancellationToken.Register(OnCancelled))
					{
						return await tcs.Task.ConfigureAwait(false);
					}
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

		private class JsonBotResult
		{
			public double Score { get; set; }

			[JsonExtensionData]
			public Dictionary<string, object> AdditionalInfo { get; } =
				new Dictionary<string, object>();
		}

		private class JsonMatchResult
		{
			public JsonBotResult[] Results { get; set; }

			[JsonExtensionData]
			public Dictionary<string, object> AdditionalInfo { get; } =
				new Dictionary<string, object>();
		}
	}
}