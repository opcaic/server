namespace OPCAIC.Worker.Services
{
	public class ExecutionConfig
	{
		/// <summary>
		///   Path to the working directory where all temporary files will be stored.
		/// </summary>
		public string WorkingDirectoryRoot { get; set; }

		/// <summary>
		///   Hard time limit for which a task can run.
		/// </summary>
		public int MaxTaskTimeout { get; set; }
	}
}
