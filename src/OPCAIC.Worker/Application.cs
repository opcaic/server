using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using OPCAIC.Worker.Services;

namespace OPCAIC.Worker
{
	/// <summary>
	///     Main class of the worker executable.
	/// </summary>
	public class Application : IHostedService
	{
		private readonly IDownloadService download;
		private readonly Worker worker;

		public Application(Worker worker, IDownloadService download)
		{
			this.worker = worker;
			this.download = download;
		}

		/// <inheritdoc />
		public Task StartAsync(CancellationToken cancellationToken)
		{
			// start async
			Task.Factory.StartNew(Run, CancellationToken.None);
			return Task.CompletedTask;
		}

		/// <inheritdoc />
		public Task StopAsync(CancellationToken cancellationToken)
		{
			// do nothing
			return Task.CompletedTask;
		}

		/// <summary>
		///     The application entry point.
		/// </summary>
		private void Run()
		{
			worker.Run();
		}
	}
}