using System.Collections.Generic;

namespace OPCAIC.GameModules.Interface
{
	public class MatchResult
	{
		/// <summary>
		///     Aggregated results for individual participating bots.
		/// </summary>
		public BotResult[] Results { get; set; }

		/// <summary>
		///     Additional key-value data.
		/// </summary>
		public Dictionary<string, object> AdditionalInfo { get; set; }
	}
}