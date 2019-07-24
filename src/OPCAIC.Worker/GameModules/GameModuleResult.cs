using System.Collections.Generic;
using Newtonsoft.Json;

namespace OPCAIC.Worker.GameModules
{
	/// <summary>
	///     Represents results of calling game module entry points.
	/// </summary>
	public enum GameModuleEntryPointResult
	{
		/// <summary>
		///     Unknown result, should never occur.
		/// </summary>
		Unknown,

		/// <summary>
		///     Stage of the game module finished successfully, the result of the stage is Success.
		/// </summary>
		Success,

		/// <summary>
		///     Stage of the game module finished successfully, the result of the stage is Failure.
		/// </summary>
		Failure,

		/// <summary>
		///     Stage of the game module failed during runtime.
		/// </summary>
		ModuleError
	}

	/// <summary>
	///     Represents full result of a stage of the game module.
	/// </summary>
	public abstract class GameModuleResult
	{
		public GameModuleEntryPointResult EntryPointResult { get; set; }
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

	/// <summary>
	///     Represents full result of the executor stage of the game module.
	/// </summary>
	public class ExecutorResult : GameModuleResult
	{
		/// <summary>
		///     Results of the match.
		/// </summary>
		public MatchResult MatchResult { get; set; }
	}

	public class MatchResult
	{
		/// <summary>
		///     Aggregated results for individual participating bots.
		/// </summary>
		public BotResult[] Results { get; set; }

		/// <summary>
		///     Additional properties.
		/// </summary>
		[JsonExtensionData]
		public Dictionary<string, object> AdditionalInfo { get; set; } =
			new Dictionary<string, object>();
	}

	public class BotResult
	{
		/// <summary>
		///     Bots score int he given match, semantics depend on the tournament format.
		/// </summary>
		public double Score { get; set; }

		/// <summary>
		///     Additional properties.
		/// </summary>
		[JsonExtensionData]
		public Dictionary<string, object> AdditionalInfo { get; set; } =
			new Dictionary<string, object>();
	}
}