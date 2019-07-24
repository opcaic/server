using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OPCAIC.Messaging.Config;
using OPCAIC.Messaging.Messages;
using OPCAIC.Worker.Config;
using OPCAIC.Worker.GameModules;

namespace OPCAIC.Broker.Runner
{
	public class Application : IHostedService
	{
		private static readonly Random rand = new Random(42);
		private readonly IBroker broker;
		private readonly AppConfig config;
		private readonly ILogger logger;
		private readonly IServiceProvider serviceProvider;
		private readonly List<Worker.Worker> workers;
		private bool stop;
		private readonly Thread thread;

		public Application(ILogger<Application> logger, IApplicationLifetime lifetime,
			IBroker broker, IServiceProvider serviceProvider, IOptions<AppConfig> config)
		{
			this.broker = broker;
			this.logger = logger;
			this.serviceProvider = serviceProvider;
			this.config = config.Value;
			workers = new List<Worker.Worker>();
			thread = new Thread(Main);
		}

		/// <inheritdoc />
		public async Task StartAsync(CancellationToken cancellationToken)
		{
			await StartWorkers();
			thread.Start();
		}

		/// <inheritdoc />
		public async Task StopAsync(CancellationToken cancellationToken)
		{
			stop = true;
			cancellationToken.Register(thread.Abort);
			await Task.WhenAll(workers.Select(w => w.StopAsync(cancellationToken)));
			thread.Join();
		}

		private async Task StartWorkers()
		{
			var ctx = new HostBuilderContext(new Dictionary<object, object>());
			ctx.Configuration = serviceProvider.GetRequiredService<IConfiguration>();
			foreach (var worker in config.WorkerSet.Workers ?? Enumerable.Empty<WorkerConfig>())
			{
				// bootstrap with custom configs
				var loggerFactory = serviceProvider.GetRequiredService<ILoggerFactory>();
				var services = new ServiceCollection()
					.AddSingleton(worker)
					.Configure<WorkerConnectorConfig>(cfg =>
					{
						cfg.Identity = worker.Identity;
						cfg.BrokerAddress = config.WorkerSet.BrokerAddress;
						cfg.HeartbeatConfig = config.Broker.HeartbeatConfig;
					})
					.AddLogging(builder
						=> builder.Services.AddSingleton<ILoggerFactory>(
							new IdentityAwareLoggerFactory(loggerFactory, worker.Identity)));

				Worker.Startup.ConfigureServices(ctx, services);
				services.Configure<ExecutionConfig>(cfg =>
				{
					cfg.WorkingDirectoryRoot = $"./wrkdir/{worker.Identity}";
					cfg.ArchiveDirectoryRoot = $"./archive/{worker.Identity}";
				});

				// replace with our custom module registry
				var registry = new DummyGameModuleRegistry();
				foreach (var game in worker.Supportedgames)
				{
					registry.AddModule(new DummyGameModule(game, loggerFactory.CreateLogger(game)));
				}

				services.RemoveAll(typeof(IGameModuleRegistry))
					.AddSingleton<IGameModuleRegistry>(registry);

				// finally, start worker
				var sp = services.BuildServiceProvider();
				var newWorker = sp.GetRequiredService<Worker.Worker>();
				workers.Add(newWorker);
				await newWorker.StartAsync(new CancellationToken());
			}
		}

		public async void Main() => RunBroker();

		private void RunBroker()
		{
			var results = new List<ReplyMessageBase>();

			var i = 0;
			broker.RegisterHandler<MatchExecutionResult>(a =>
			{
				logger.LogInformation($"Finished: {a.Id}");
				results.Add(a);
			});

			while (!stop && i < 200)
			{
				Thread.Sleep(50);
				if (broker.GetUnfinishedTasksCount() > 20)
				{
					continue;
				}

				try
				{
					i++;
					broker.EnqueueWork(new MatchExecutionRequest
					{
						Game = config.Games[rand.Next(config.Games.Length)], Id = Guid.NewGuid()
					});
				}
				catch (Exception e)
				{
					Console.WriteLine("Exception caught");
					Console.WriteLine(e);
				}
			}

			while (!stop && results.Count < i)
			{
				Thread.Sleep(100);
			}

			Console.WriteLine($"Completed: {results.Count}/200 tasks");
		}
	}
}