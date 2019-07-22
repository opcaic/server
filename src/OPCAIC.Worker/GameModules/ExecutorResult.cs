using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace OPCAIC.Worker.GameModules
{
	/// <summary>
	/// Represents full result of the executor stage of the game module.
	/// </summary>
	public class ExecutorResult : GameModuleResult
	{
		/// <summary>
		/// Result of a match, semantics of the value is up to the game itself. 
		/// </summary>
		[JsonProperty("matchResult")]
		public double MatchResult { get; set; }

		/// <summary>
		/// Any additional info about the match executed.
		/// </summary>
		[JsonProperty("additionalInfo")]
		public Dictionary<string, object> additionalInfo { get; set; } =
			new Dictionary<string, object>();
	}
}