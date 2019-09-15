using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OPCAIC.Common;
using OPCAIC.GameModules.Interface;
using OPCAIC.Utils;
using OPCAIC.Worker.Config;
using OPCAIC.Worker.Exceptions;
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
			return module ??
				throw new GameModuleNotFoundException($"Game module '{game}' was not found");
		}

		/// <inheritdoc />
		public IGameModule TryFindGameModule(string game)
		{
			modules.TryGetValue(game, out var module);
			return module;
		}

		/// <inheritdoc />
		public IEnumerable<IGameModule> GetAllModules()
		{
			return modules.Values;
		}

		/// <summary>
		///     Loads game modules from specified directory.
		/// </summary>
		/// <param name="modulePath">Path to game modules.</param>
		private void LoadModules(string modulePath)
		{
			logger.LogInformation($"Loading game modules from folder '{modulePath}'");
			var moduleDirectory = Directory.CreateDirectory(modulePath);
			foreach (var directory in moduleDirectory.GetDirectories())
			{
				logger.LogInformation($"Loading game module from '{directory.FullName}'");
				var gameName = Path.GetFileName(directory.FullName);
				var configFile = Path.Combine(directory.FullName,
					Constants.FileNames.EntryPointsConfig);

				ExternalGameModuleConfiguration config;

				try
				{
					using (var sr = new StreamReader(configFile))
					{
						config = JsonHelper.DeserializeStrict<ExternalGameModuleConfiguration>(
							sr.ReadToEnd());
					}
				}
				catch (Exception e)
				{
					logger.LogError(LoggingEvents.Startup, e,
						$"Failed to load game module configuration '{configFile}'");
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