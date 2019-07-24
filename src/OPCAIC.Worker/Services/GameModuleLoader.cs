using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection.Metadata.Ecma335;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OPCAIC.Worker.Config;
using OPCAIC.Worker.GameModules;

namespace OPCAIC.Worker.Services
{
	/// <summary>
	///     Class for loading instances of <see cref="ExternalGameModule" />
	/// </summary>
	public class GameModuleLoader : IGameModuleRegistry
	{

		private readonly ILogger<GameModuleLoader> logger;
		private readonly ILoggerFactory loggerFactory;

		private readonly Dictionary<string, ExternalGameModule> modules;

		public GameModuleLoader(ILogger<GameModuleLoader> logger, ILoggerFactory loggerFactory,
			IOptions<Configuration> config)
		{
			this.loggerFactory = loggerFactory;
			this.logger = logger;
			modules = new Dictionary<string, ExternalGameModule>();
			LoadModules(config.Value.ModulePath);
		}

		/// <inheritdoc />
		public IGameModule FindGameModule(string game)
		{
			modules.TryGetValue(game, out var module);
			return module;
		}

		/// <inheritdoc />
		public IEnumerable<IGameModule> GetAllModules() => modules.Values;

		/// <summary>
		///     Loads game modules from specified directory.
		/// </summary>
		/// <param name="modulePath">Path to game modules.</param>
		private void LoadModules(string modulePath)
		{
			var moduleDirectory = Directory.CreateDirectory(modulePath);
			foreach (var directory in moduleDirectory.GetDirectories())
			{
				var gameName = Path.GetFileName(directory.FullName);
				var configFile = Path.Combine(directory.FullName,
					Constants.FileNames.ModuleConfig);

				GameModuleConfiguration config;

				try
				{
					using (var sr = new StreamReader(configFile))
					{
						config = JsonHelper.DeserializeStrict<GameModuleConfiguration>(
							sr.ReadToEnd());
					}
				}
				catch (Exception e)
				{
					logger.LogError(e, $"Failed to load game module configuration '{configFile}'");
					continue;
				}

				modules.Add(gameName, new ExternalGameModule(
					loggerFactory.CreateLogger($"{nameof(ExternalGameModule)}:{gameName}"),
					config,
					gameName,
					directory.FullName));
			}
		}
	}
}