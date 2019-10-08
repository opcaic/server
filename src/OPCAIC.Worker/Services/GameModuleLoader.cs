using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OPCAIC.Common;
using OPCAIC.GameModules.Interface;
using OPCAIC.Worker.Config;
using OPCAIC.Worker.Exceptions;
using OPCAIC.Worker.GameModules;

namespace OPCAIC.Worker.Services
{
	/// <summary>
	///     Class for loading instances of <see cref="ExternalGameModule" />
	/// </summary>
	public class GameModuleLoader : IGameModuleRegistry, IGameModuleWatcher, IDisposable
	{
		private readonly ILogger<GameModuleLoader> logger;
		private readonly ILoggerFactory loggerFactory;
		private readonly string modulePath;
		private readonly FileSystemWatcher watcher;

		public GameModuleLoader(ILogger<GameModuleLoader> logger, ILoggerFactory loggerFactory,
			IOptions<Configuration> config)
		{
			this.loggerFactory = loggerFactory;
			this.logger = logger;
			modulePath = config.Value.ModulePath;
			watcher = ConfigureWatcher(modulePath);
		}

		/// <inheritdoc />
		public void Dispose()
		{
			watcher?.Dispose();
		}

		/// <inheritdoc />
		public IGameModule FindGameModule(string game)
		{
			return LoadModule(modulePath, game);
		}

		/// <inheritdoc />
		public IEnumerable<string> GetAllModuleNames()
		{
			return Directory.CreateDirectory(modulePath).GetDirectories().Select(d => d.Name);
		}

		/// <inheritdoc />
		public event EventHandler ModuleListChanged;

		private FileSystemWatcher ConfigureWatcher(string path)
		{
			// make sure the directory exists
			Directory.CreateDirectory(path);

			var w = new FileSystemWatcher(path);

			// only top level is interesting to us
			w.IncludeSubdirectories = false;

			void OnChange(object sender, FileSystemEventArgs e)
			{
				ModuleListChanged?.Invoke(this, EventArgs.Empty);
			}

			// only add/delete of the game module
			w.Created += OnChange;
			w.Deleted += OnChange;

			w.EnableRaisingEvents = true;
			return w;
		}

		private ExternalGameModule LoadModule(string directory, string gameName)
		{
			var rootDir = Path.Combine(directory, gameName);
			logger.LogInformation($"Loading game module '{gameName}'");
			var configFile = Path.Combine(directory, gameName,
				Constants.FileNames.EntryPointsConfig);

			ExternalGameModuleConfiguration config;

			using (var sr = new StreamReader(configFile))
			{
				config = JsonHelper.DeserializeStrict<ExternalGameModuleConfiguration>(
					sr.ReadToEnd());

				return new ExternalGameModule(
					loggerFactory.CreateLogger($"{nameof(ExternalGameModule)}:{gameName}"),
					config,
					gameName,
					rootDir);
			}
		}
	}
}