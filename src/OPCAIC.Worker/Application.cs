using System;
using System.Threading.Tasks;
using OPCAIC.Worker.Services;

namespace OPCAIC.Worker
{
	/// <summary>
	///   Main class of the worker executable.
	/// </summary>
	public class Application
	{
		private readonly Worker worker;
		private readonly IDownloadService download;

		public Application(Worker worker, IDownloadService download)
		{
			this.worker = worker;
			this.download = download;
		}

		/// <summary>
		///   The application entrypoint.
		/// </summary>
		public void Run()
		{
			worker.Run();
		}

		private async Task TestDownloader()
		{
			await download.UploadAsync("/attachments/b.a", "a.a", false);
			await download.UploadAsync("/exercises", "a.a");
			await download.UploadAsync("/results/b.a", "a.a", false);
			await download.UploadAsync("/submissions/b.a", "a.a");
			await download.DownloadAsync("/exercises/da39a3ee5e6b4b0d3255bfef95601890afd80709", "b.a");
			Console.WriteLine("Success");
		}
	}
}
