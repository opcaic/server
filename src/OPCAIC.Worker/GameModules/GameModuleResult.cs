using System;
using System.IO;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace OPCAIC.Worker.GameModules
{
	/// <summary>
	/// Represents results of calling game module entrypoints.
	/// </summary>
	public enum GameModuleEntrypointResult
	{
		/// <summary>
		/// Stage of the game module finished successfully, the result of the stage is Failure.
		/// </summary>
		Failure,

		/// <summary>
		/// Stage of the game module finished successfully, the result of the stage is Success.
		/// </summary>
		Success,

		/// <summary>
		/// Stage of the game module finished successfully, but the resulting file could not be found.
		/// </summary>
		OutputNotFoundError,

		/// <summary>
		/// Stage of the game module finished successfully, but the resulting file is in a wrong format.
		/// </summary>
		OutputFormatError,

		/// <summary>
		/// Stage of the game module could not be started.
		/// </summary>
		ModuleStartError,

		/// <summary>
		/// Stage of the game module failed during runtime.
		/// </summary>
		ModuleRuntimeError,

		/// <summary>
		/// Stage of the game module was killed.
		/// </summary>
		ModuleKilledError,

		/// <summary>
		/// Stage of the game module finished successfully, the complete result is to-be-read from result file.
		/// </summary>
		Incomplete
	}

	/// <summary>
	/// Represents full result of a stage of the game module.
	/// </summary>
	public class GameModuleResult
	{
		[JsonProperty("result")]
		[JsonConverter(typeof(StringEnumConverter))]
		public GameModuleEntrypointResult EntrypointResult { get; set; }

		[JsonProperty("message")]
		public string Message { get; set; }

		[JsonProperty("log")]
		public string Log { get; set; }

		/// <summary>
		/// Deserializes GameModuleResult object from json.
		/// </summary>
		/// <typeparam name="T">Any children class of GameModuleResult.</typeparam>
		/// <param name="filePath">Path to the json.</param>
		/// <returns>Deserializes GameModuleObject.</returns>
		public static T GetGameModuleOutputFromJson<T>(string filePath)
			where T : GameModuleResult, new()
		{
			T gmOutput = new T();
			if (!File.Exists(filePath))
			{
				gmOutput.EntrypointResult = GameModuleEntrypointResult.OutputNotFoundError;
				return gmOutput;
			}

			string json = File.ReadAllText(filePath);
			gmOutput = JsonConvert.DeserializeObject<T>(json);
			if (gmOutput == null)
			{
				gmOutput = new T();
				gmOutput.EntrypointResult = GameModuleEntrypointResult.OutputFormatError;
			}

			return gmOutput;
		}
	}

	#region Children classes for outputs of the first three stages of a game module.

	public class CompilerResult : GameModuleResult
	{
	}

	public class ValidatorResult : GameModuleResult
	{
	}

	public class CheckerResult : GameModuleResult
	{
	}

	#endregion
}