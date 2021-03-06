﻿using System.IO;
using Newtonsoft.Json.Linq;

namespace OPCAIC.GameModules.Interface
{
	/// <summary>
	///     Configuration of a game module when executing a particular entry point.
	/// </summary>
	public class EntryPointConfiguration
	{
		/// <summary>
		///     Path to directory containing any additional files which may be needed by the game module.
		/// </summary>
		public DirectoryInfo AdditionalFiles { get; set; }

		/// <summary>
		///     Configuration for the game module to use for given task.
		/// </summary>
		public JObject Configuration { get; set; }
	}
}