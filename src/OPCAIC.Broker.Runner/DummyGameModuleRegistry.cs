using System.Collections.Generic;
using OPCAIC.Worker.GameModules;

namespace OPCAIC.Broker.Runner
{
	/// <summary>
	///     Aggregates existing game modules in the worker
	/// </summary>
	public class DummyGameModuleRegistry : IGameModuleRegistry
	{
		private readonly Dictionary<string, IGameModule> modules;

		public DummyGameModuleRegistry() => modules = new Dictionary<string, IGameModule>();

		/// <inheritdoc />
		public IGameModule FindGameModule(string game)
		{
			modules.TryGetValue(game, out var module);
			return module;
		}

		/// <inheritdoc />
		public IEnumerable<IGameModule> GetAllModules() => modules.Values;

		/// <summary>
		///     Adds a module to the list of existing game modules.
		/// </summary>
		/// <param name="module"></param>
		public void AddModule(IGameModule module) => modules.Add(module.GameName, module);
	}
}