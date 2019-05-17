namespace OPCAIC.Worker
{
	/// <summary>
	///   Main class of the worker executable.
	/// </summary>
	public class Application
	{
		private readonly Worker worker;

		public Application(Worker worker) => this.worker = worker;

		/// <summary>
		///   The application entrypoint.
		/// </summary>
		public void Run() => worker.Run();
	}
}
