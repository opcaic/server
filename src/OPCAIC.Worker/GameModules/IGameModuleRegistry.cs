using System;
using System.Collections.Generic;
using log4net;
using Microsoft.Extensions.Logging;

namespace OPCAIC.Worker.GameModules
{
	public interface IGameModuleRegistry
	{
		IGameModule FindGameModule(string game);
		IEnumerable<IGameModule> GetAllModules();
	}
}
