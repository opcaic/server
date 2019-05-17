using System.Collections.Generic;
using System.Linq;

namespace OPCAIC.Worker.GameModules
{
	public class DummyModuleRegistry : IGameModuleRegistry
	{
		private readonly Dictionary<string, IGameModule> modules;

		public DummyModuleRegistry(string[] games)
			=> modules = games.Select(game => new DummyGameModule(
					game))
				.Cast<IGameModule>()
				.ToDictionary(m => m.GameName);

		public IGameModule FindGameModule(string game) => modules[game];

		public IEnumerable<IGameModule> GetAllModules() => modules.Values;
	}
}
