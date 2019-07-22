using System.IO;
using Chimera.Extensions.Logging.Log4Net;
using log4net;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using OPCAIC.Messaging;
using OPCAIC.Messaging.Config;
using OPCAIC.Messaging.Messages;
using OPCAIC.Worker.Config;
using OPCAIC.Worker.GameModules;
using OPCAIC.Worker.Services;

namespace OPCAIC.Worker
{
	/// <summary>
	///   Configuration class for the hosted console application.
	/// </summary>
	public static class Startup
	{
		/// <summary>
		///   Configures the logging facilities.
		/// </summary>
		/// <param name="loggingBuilder"></param>
		public static void ConfigureLogging(HostBuilderContext host, ILoggingBuilder loggingBuilder) => loggingBuilder.AddLog4NetLogging();

		/// <summary>
		///   Configures logging builder to use Log4Net logging.
		/// </summary>
		/// <param name="logging"></param>
		/// <returns></returns>
		private static ILoggingBuilder AddLog4NetLogging(this ILoggingBuilder logging)
		{
			var container = new Log4NetContainer(new Log4NetSettings
			{
				ConfigFilePath = "log4net.config",
				Watch = true
			});
			container.Initialize();
			return logging.AddProvider(new Log4NetProvider(container));
		}

		/// <summary>
		///   Configures the application services.
		/// </summary>
		/// <param name="host">Host configuration.</param>
		/// <param name="services">Services collection to configure.</param>
		public static void ConfigureServices(HostBuilderContext host, IServiceCollection services)
		{
			var config = host.Configuration;

			services
				.AddOptions()
				.Configure<FileServerConfig>(config.GetSection("FileServer"))
				.Configure<ExecutionConfig>(config.GetSection("Execution"));

			services
				.AddWorker(worker => config.Bind("ConnectorConfig", worker))
				.AddTransient<
					IJobExecutor<MatchExecutionRequest, MatchExecutionResult>,
					MatchExecutor>()
				.AddTransient<
					IJobExecutor<SubmissionValidationRequest, SubmissionValidationResult>,
					SubmissionValidator>()
				.AddTransient<IDownloadService, DownloadService>()
				.AddTransient<IExecutionServices, ExecutionServices>();

			var registry = new GameModuleRegistry();
			services.AddSingleton<IGameModuleRegistry>(registry);
			var modulePath = config.GetValue<string>("ModulePath");
			LoadModules(registry, modulePath);
		}

		/// <summary>
		///   Loads all game modules from specified path into provided registry
		/// </summary>
		/// <param name="registry">Registry into which loaded game modules should be added.</param>
		/// <param name="path">Path to root directory of the game modules.</param>
		private static void LoadModules(GameModuleRegistry registry, string path)
		{
			Directory.CreateDirectory(path);
			foreach (var directory in Directory.GetDirectories(path))
			{
				string directoryName = Path.GetDirectoryName(directory);
				var config = JsonConvert.DeserializeObject<EntrypointsConfiguration>(directoryName +
					Path.DirectorySeparatorChar +
					"entrypoints.json");

				registry.AddModule(new ExternalGameModule(
					new Log4NetLogger(LogManager.GetLogger(typeof(ExternalGameModule))),
					config,
					Path.GetDirectoryName(path),
					path));
			}
		}
	}
}