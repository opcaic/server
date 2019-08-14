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

		/// <summary>
		///     The application entry point.
		/// </summary>
		public void Run()
		{
			worker.Run();
		}
	}
}