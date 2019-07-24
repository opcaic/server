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
		private async void Run()
		{
			await download.UploadAsync("/attachments/b.a", "a.a", false);
			await download.UploadAsync("/exercises", "a.a");
			await download.UploadAsync("/results/b.a", "a.a", false);
			await download.UploadAsync("/submissions/b.a", "a.a");
			await download.DownloadAsync("/exercises/da39a3ee5e6b4b0d3255bfef95601890afd80709", "b.a");
			Console.WriteLine("Success");
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
