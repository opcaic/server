using System;
using System.Collections.Generic;
using OPCAIC.GameModules.Interface;

namespace OPCAIC.Worker.GameModules
{
	/// <summary>
	///     Provides methods for getting <see cref="IGameModule" /> instances for a given games.
	/// </summary>
	public interface IGameModuleRegistry
	{
		/// <summary>
		///     Finds a game module based on the game name.
		/// </summary>
		/// <param name="game">The name of the game.</param>
		/// <exception cref="InvalidOperationException">If game with given name was not found.</exception>
		/// <returns></returns>
		IGameModule FindGameModule(string game);

		/// <summary>
		///     Finds a game module based on the game name. Returns null if not found.
		/// </summary>
		/// <returns></returns>
		IGameModule TryFindGameModule(string game);

		/// <summary>
		///     Gets all game modules.
		/// </summary>
		/// <returns></returns>
		IEnumerable<IGameModule> GetAllModules();
	}
}