using System.Collections.Generic;

namespace OPCAIC.Worker.GameModules
{
	/// <summary>
	///   Provides methods for getting <see cref="IGameModule" /> instances for a given games.
	/// </summary>
	public interface IGameModuleRegistry
	{
		/// <summary>
		///   Finds a game module based on the game name.
		/// </summary>
		/// <param name="game">The name of the game.</param>
		/// <returns></returns>
		IGameModule FindGameModule(string game);

		/// <summary>
		///   Gets all game modules.
		/// </summary>
		/// <returns></returns>
		IEnumerable<IGameModule> GetAllModules();
	}
}
