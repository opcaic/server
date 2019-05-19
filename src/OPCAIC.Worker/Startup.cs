using System.IO;
using Chimera.Extensions.Logging.Log4Net;
using log4net;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OPCAIC.Messaging;
using OPCAIC.Messaging.Config;
using OPCAIC.Messaging.Messages;
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
		/// <param name="loggerFactory"></param>
		public static void ConfigureLogging(ILoggerFactory loggerFactory) => loggerFactory.AddLog4Net();

		/// <summary>
		///   Configures the configuration facilities.
		/// </summary>
		/// <param name="configuration">Configuration root of the entire application.</param>
		/// <param name="services">Service collection of the application.</param>
		public static void Configure(IConfiguration configuration, IServiceCollection services)
		{
			services
				.AddOptions()
				.Configure<WorkerConnectorConfig>(configuration.GetSection("ConnectorConfig"));

			var registry = new GameModuleRegistry();
			services.AddSingleton<IGameModuleRegistry>(registry);
			var modulePath = configuration.GetValue<string>("ModulePath");
			LoadModules(registry, modulePath);
		}

		/// <summary>
		///   Configures used services for the application.
		/// </summary>
		/// <param name="services">Service collection of the application.</param>
		public static void ConfigureServices(IServiceCollection services)
			=> services
				.AddSingleton<WorkerConnector>()
				.AddSingleton<Worker>()
				.AddTransient<
					IJobExecutor<MatchExecutionRequest, MatchExecutionResult>,
					MatchExecutor>()
				.AddTransient<
					IJobExecutor<SubmissionValidationRequest, SubmissionValidationResult>,
					SubmissionValidator>();

		/// <summary>
		///   Loads all game modules from specified path into provided registry
		/// </summary>
		/// <param name="registry">Registry into which loaded game modules should be added.</param>
		/// <param name="path">Path to root directory of the game modules.</param>
		internal static void LoadModules(GameModuleRegistry registry, string path)
		{
			Directory.CreateDirectory(path);
			foreach (var directory in Directory.GetDirectories(path))
				registry.AddModule(new ExternalGameModule(
					new Log4NetLogger(LogManager.GetLogger(typeof(ExternalGameModule))),
					Path.GetDirectoryName(path),
					path));
		}
	}
}
