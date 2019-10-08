using System;
using System.Collections.Generic;

namespace OPCAIC.Worker.GameModules
{
	/// <summary>
	///     Provides an event for notifying when set of available game modules changes.
	/// </summary>
	public interface IGameModuleWatcher
	{
		/// <summary>
		///     Fired when list of available game modules changes.
		/// </summary>
		event EventHandler ModuleListChanged;
	}
}