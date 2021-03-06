﻿using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace OPCAIC.ApiService.Services
{
	public abstract class HostedJob : IHostedService
	{
		private readonly CancellationTokenSource cancellationTokenSource;
		protected ILogger Logger { get; }
		protected TimeSpan Delay { get; set;  }

		private readonly IServiceScopeFactory scopeFactory;
		private Task executionTask;
		private readonly Task stopTask;
		private readonly TaskCompletionSource<object> stopTaskSource;

		protected HostedJob(IServiceScopeFactory scopeFactory, ILogger logger, TimeSpan delay)
		{
			this.scopeFactory = scopeFactory;
			cancellationTokenSource = new CancellationTokenSource();
			Delay = delay;
			stopTaskSource = new TaskCompletionSource<object>();
			stopTask = stopTaskSource.Task;
			Logger = logger;
		}

		protected virtual Task Init(IServiceProvider services, CancellationToken cancellationToken)
		{
			return Task.CompletedTask;
		}

		public async Task StartAsync(CancellationToken cancellationToken)
		{
			Logger.LogInformation($"{GetType().Name} - starting.");
			await ScopeExecute(Init, cancellationToken);

			executionTask = Task.Run(() => ExecuteAsync(cancellationTokenSource.Token), CancellationToken.None);
			Logger.LogInformation($"{GetType().Name} - started.");
		}

		public async Task StopAsync(CancellationToken cancellationToken)
		{
			Logger.LogInformation($"{GetType().Name} - stopping.");
			cancellationToken.Register(cancellationTokenSource.Cancel);
			stopTaskSource.SetResult(null); // signal stop
			await executionTask;
			Logger.LogInformation($"{GetType().Name} - stopped.");
		}

		protected abstract Task ExecuteJob(IServiceProvider scopedProvider,
			CancellationToken cancellationToken);

		private async Task ExecuteAsync(CancellationToken cancellationToken)
		{
			while (!stopTask.IsCompleted)
			{
				await ScopeExecute(ExecuteJob, cancellationToken);

				// will wake either after the delay or when StopAsync is called
				await Task.WhenAny(Task.Delay(Delay, cancellationToken), stopTask);
			}
		}

		protected async Task ScopeExecute(Func<IServiceProvider, CancellationToken, Task> action, CancellationToken cancellationToken = default)
		{
			var scope = scopeFactory.CreateScope();
			try
			{
				await action(scope.ServiceProvider, cancellationToken);
			}
			catch (Exception e) when (Log(e))
			{ 
				// already logged in Log(e)
			}
			finally
			{
				scope.Dispose();
			}
		}

		private bool Log(Exception e)
		{
			Logger.LogError(e, $"An error has occured during execution of {GetType().Name}");
			return true;
		}
	}
}