namespace OPCAIC.Worker.Config
{
	public class ExecutionConfig
	{
		/// <summary>
		///     Path to the working directory where all temporary files will be stored.
		/// </summary>
		public string WorkingDirectoryRoot { get; set; }

		/// <summary>
		///     Path to the directory where files from finished tasks should be archived.
		/// </summary>
		public string ArchiveDirectoryRoot { get; set; }

		/// <summary>
		///     Hard time limit in seconds for which a task can run.
		/// </summary>
		public int MaxTaskTimeoutSeconds { get; set; } = 60;
	}
}