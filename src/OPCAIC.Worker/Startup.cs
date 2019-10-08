using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using OPCAIC.Messaging.Messages;
using OPCAIC.Utils;
using OPCAIC.Worker.Config;
using OPCAIC.Worker.GameModules;
using OPCAIC.Worker.Services;

namespace OPCAIC.Worker
{
	/// <summary>
	///     Configuration class for the hosted console application.
	/// </summary>
	public static class Startup
	{
		/// <summary>
		///     Configures the application services.
		/// </summary>
		/// <param name="host">Host configuration.</param>
		/// <param name="services">Services collection to configure.</param>
		public static void ConfigureServices(HostBuilderContext host, IServiceCollection services)
		{
			var config = host.Configuration;

			services
				.AddOptions()
				.Configure<Configuration>(config)
				.Configure<FileServerConfig>(config.GetSection(nameof(Configuration.FileServer)))
				.Configure<ExecutionConfig>(config.GetSection(nameof(Configuration.Execution)));

			services
				.AddWorker(worker => config.Bind(nameof(Configuration.ConnectorConfig), worker))
				.AddTransient<
					IJobExecutor<MatchExecutionRequest, MatchExecutionResult>,
					MatchExecutor>()
				.AddTransient<
					IJobExecutor<SubmissionValidationRequest, SubmissionValidationResult>,
					SubmissionValidator>()
				.AddSingleton<IDownloadServiceFactory, DownloadServiceFactory>()
				.AddTransient<IExecutionServices, ExecutionServices>()
				.AddSingleton<IGameModuleRegistry, GameModuleLoader>()
				.AddSingleton(
					sp => (IGameModuleWatcher)sp.GetRequiredService<IGameModuleRegistry>());
		}
	}
}