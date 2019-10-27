namespace OPCAIC.Worker.Config
{
	public class ExecutionConfig
	{
		/// <summary>
		///     Path to the working directory where all temporary files will be stored.
		/// </summary>
		public string WorkingDirectory { get; set; }

		/// <summary>
		///     Path to the directory where files from finished tasks should be archived.
		/// </summary>
		public string ArchiveDirectory { get; set; }

		/// <summary>
		///     Path to the directory where files from failed tasks should be archived.
		/// </summary>
		public string ErrorDirectory { get; set; }

		/// <summary>
		///     How many days to keep the archived directories in <see cref="ArchiveDirectory"/>.
		/// </summary>
		public int ArchiveRetentionDays { get; set; } = 30;

		/// <summary>
		///     How many days to keep the archived directories in <see cref="ErrorDirectory"/>.
		/// </summary>
		public int ErrorRetentionDays { get; set; } = 30;

		/// <summary>
		///     Hard time limit in seconds for which a task can run.
		/// </summary>
		public int MaxTaskTimeoutSeconds { get; set; } = 60;
	}
}