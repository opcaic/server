using System.IO;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using OPCAIC.Worker.Services;

namespace OPCAIC.Worker.GameModules
{
	class EntrypointsConfiguration
	{
		[JsonProperty("CheckerEntrypoint")] public string CheckerEntrypoint;
		[JsonProperty("CompilerEntrypoint")] public string CompilerEntrypoint;
		[JsonProperty("ValidatorEntrypoint")] public string ValidatorEntrypoint;
		[JsonProperty("ExecutorEntrypoint")] public string ExecutorEntrypoint;
	}

	/// <summary>
	/// Represents an external game module, executed as an external script.
	/// </summary>
	internal class ExternalGameModule : IGameModule
	{
		private readonly string rootDir;
		private readonly ILogger logger;
		private readonly EntrypointsConfiguration entrypointsConfig;

		/// <summary>
		/// Creates a new external game module object.
		/// </summary>
		/// <param name="logger">Logger of the game module.</param>
		/// <param name="gameName">Name of the game represented by the module.</param>
		/// <param name="rootDir">Root (working) directory of the game.</param>
		public ExternalGameModule(ILogger logger, EntrypointsConfiguration config, string gameName,
			string rootDir)
		{
			this.logger = logger;
			this.rootDir = rootDir;
			GameName = gameName;
			entrypointsConfig = config;
		}

		public string GameName { get; }

		/// <summary>
		/// Invokes game modules entrypoint and returns its result.
		/// </summary>
		/// <param name="rootDir">Root directory of the game module.</param>
		/// <param name="fileName">File name of the entrypoint to run.</param>
		/// <param name="args">Arguments of the entrypoint.</param>
		/// <returns>Complete or incomplete result of the game module, depending on the result of the entrypoint process.</returns>
		private GameModuleResult InvokeGameModuleEntrypoint(string rootDir, string fileName,
			string args)
		{
			GameModuleResult result = new GameModuleResult();
			try
			{
				logger.LogInformation($"Invoking entrypoint {fileName} with args ({args})");
				var task = ProcessHandler.RunProcessAsync(rootDir, fileName, args);
				switch (task.Result)
				{
					case ProcessResult.Failed:
						result.Log =
							$"Game module entrypoint {fileName} ended with a nonzero exitcode.";
						logger.LogError(
							$"Game module entrypoint {fileName} ended with a nonzero exitcode.");
						result.EntrypointResult = GameModuleEntrypointResult.ModuleRuntimeError;
						break;
					case ProcessResult.Killed:
						result.Log = $"Game module entrypoint {fileName} was killed.";
						logger.LogError($"Game module entrypoint {fileName} was killed.");
						result.EntrypointResult = GameModuleEntrypointResult.ModuleKilledError;
						break;
					default:
						logger.LogInformation(
							$"Game module entrypoint {fileName} ended successfully.");
						result.EntrypointResult = GameModuleEntrypointResult.Incomplete;
						break;
				}
			}
			catch
			{
				result.Log =
					$"Game module entrypoint {fileName} could not be invoked with given args ({args}).";
				logger.LogError(
					$"Game module entrypoint {fileName} could not be invoked with given args ({args}).");
				result.EntrypointResult = GameModuleEntrypointResult.ModuleStartError;
			}

			return result;
		}

		public void Clean()
		{
		}

		/// <inheritdoc/>
		public CheckerResult Check(string inputSrcDir, string outputDir)
		{
			var processArgs = inputSrcDir + " " + outputDir;
			var checkerName = entrypointsConfig.CheckerEntrypoint;

			var entrypointRes = InvokeGameModuleEntrypoint(rootDir, checkerName, processArgs);

			if (entrypointRes.EntrypointResult == GameModuleEntrypointResult.Incomplete)
			{
				return GameModuleResult.GetGameModuleOutputFromJson<CheckerResult>(outputDir +
					Path.DirectorySeparatorChar +
					Constants.FileNames.CheckerResult);
			}
			else return (CheckerResult)entrypointRes;
		}

		/// <inheritdoc/>
		public ValidatorResult Validate(string inputBinDir, string outputDir)
		{
			var processArgs = inputBinDir + " " + outputDir;
			var validatorName = entrypointsConfig.ValidatorEntrypoint;

			var entrypointRes = InvokeGameModuleEntrypoint(rootDir, validatorName, processArgs);

			if (entrypointRes.EntrypointResult == GameModuleEntrypointResult.Incomplete)
			{
				return GameModuleResult.GetGameModuleOutputFromJson<ValidatorResult>(outputDir +
					Path.DirectorySeparatorChar +
					Constants.FileNames.ValidatorResult);
			}
			else return (ValidatorResult)entrypointRes;
		}

		/// <inheritdoc/>
		public CompilerResult Compile(string inputSrcDir, string outputBinDir, string outputDir)
		{
			var processArgs = inputSrcDir + " " + outputBinDir + " " + outputDir;
			var compilerName = entrypointsConfig.CompilerEntrypoint;

			var entrypointRes = InvokeGameModuleEntrypoint(rootDir, compilerName, processArgs);

			if (entrypointRes.EntrypointResult == GameModuleEntrypointResult.Incomplete)
			{
				return GameModuleResult.GetGameModuleOutputFromJson<CompilerResult>(outputDir +
					Path.DirectorySeparatorChar +
					Constants.FileNames.CompilerResult);
			}
			else return (CompilerResult)entrypointRes;
		}

		/// <inheritdoc/>
		public ExecutorResult Execute(string[] inputDirs, string outputDir)
		{
			var processArgs = "";
			// 1..N participating bots
			foreach (string inputDir in inputDirs)
			{
				processArgs += inputDir + " ";
			}

			processArgs += outputDir;

			var executorName = entrypointsConfig.ExecutorEntrypoint;
			var entrypointRes = InvokeGameModuleEntrypoint(rootDir, executorName, processArgs);

			if (entrypointRes.EntrypointResult == GameModuleEntrypointResult.Incomplete)
			{
				return GameModuleResult.GetGameModuleOutputFromJson<ExecutorResult>(outputDir +
					Path.DirectorySeparatorChar +
					Constants.FileNames.ExecutorResult);
			}
			else return (ExecutorResult)entrypointRes;
		}
	}
}