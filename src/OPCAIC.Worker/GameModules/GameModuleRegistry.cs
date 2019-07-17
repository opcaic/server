using System.Collections.Generic;

namespace OPCAIC.Worker.GameModules
{
	/// <summary>
	///   Aggregates existing game modules in the worker
	/// </summary>
	public class GameModuleRegistry : IGameModuleRegistry
	{
		private readonly Dictionary<string, IGameModule> modules;

		public GameModuleRegistry() => modules = new Dictionary<string, IGameModule>();

		/// <inheritdoc />
		public IGameModule FindGameModule(string game)
		{
			modules.TryGetValue(game, out var module);
			return module;
		}

		/// <inheritdoc />
		public IEnumerable<IGameModule> GetAllModules() => modules.Values;

		/// <summary>
		///   Adds a module to the list of existing game modules.
		/// </summary>
		/// <param name="module"></param>
		public void AddModule(IGameModule module) => modules.Add(module.GameName, module);
	}
}