using System.Collections.Generic;

namespace OPCAIC.Broker.Runner
{
	public static class WorkerEntryExtensions
	{
		public static bool HasRuntimes(this WorkerEntry worker, IEnumerable<string> runtimes)
		{
			foreach (var runtime in runtimes)
			{
				if (!worker.Capabilities.SupportedLanguages.Contains(runtime))
				{
					return false;
				}
			}

			return true;
		}

		public static bool HasGame(this WorkerEntry worker, string game)
		{
			return worker.Capabilities.SupportedGames.Contains(game);
		}
	}
}