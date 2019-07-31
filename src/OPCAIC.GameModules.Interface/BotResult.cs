using System.Collections.Generic;

namespace OPCAIC.GameModules.Interface
{
	/// <summary>
	///     Aggregates results of a single bot in a match.
	/// </summary>
	public class BotResult
	{
		/// <summary>
		///     Bots score int he given match, semantics depend on the tournament format.
		/// </summary>
		public double Score { get; set; }

		/// <summary>
		///     Flag whether the bot crashed during the execution.
		/// </summary>
		public bool HasCrashed { get; set; }

		/// <summary>
		///     Additional key-value data.
		/// </summary>
		public Dictionary<string, object> AdditionalInfo { get; set; }
	}
}