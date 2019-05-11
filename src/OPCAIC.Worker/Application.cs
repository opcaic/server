namespace OPCAIC.Worker
{
	public class Application
	{
		private readonly Worker worker;

		public Application(Worker worker) => this.worker = worker;

		public void Run() => worker.Run();
	}
}
