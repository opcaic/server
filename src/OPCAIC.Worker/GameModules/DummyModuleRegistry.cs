using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace OPCAIC.Worker.GameModules
{
	class DummyModuleRegistry : IGameModuleRegistry
	{
		private Dictionary<string, IGameModule> modules;

		public DummyModuleRegistry(IServiceProvider serviceProvider, WorkerConfig config)
		{
			modules = config.Supportedgames.Select(game => new DummyGameModule(
					serviceProvider.GetRequiredService<ILogger<DummyGameModule>>(),
					game))
				.Cast<IGameModule>()
				.ToDictionary(m => m.GameName);
		}

		public IGameModule FindGameModule(string game)
		{
			return modules[game];
		}

		public IEnumerable<IGameModule> GetAllModules()
		{
			return modules.Values;
		}
	}
}