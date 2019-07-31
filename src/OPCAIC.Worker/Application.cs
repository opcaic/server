using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using OPCAIC.Worker.Services;

namespace OPCAIC.Worker
{
	/// <summary>
	///   Main class of the worker executable.
	/// </summary>
	public class Application : IHostedService
	{
		private readonly Worker worker;
		private readonly IDownloadService download;

		public Application(Worker worker, IDownloadService download)
		{
			this.worker = worker;
			this.download = download;
		}

		/// <summary>
		///   The application entry point.
		/// </summary>
		public void Run()
		{
			worker.Run();
		}

		/// <inheritdoc />
		public async Task StartAsync(CancellationToken cancellationToken)
		{
			// start async
			Task.Factory.StartNew(Run);
		}

		/// <inheritdoc />
		public async Task StopAsync(CancellationToken cancellationToken)
		{
			// do nothing
		}
	}
}
