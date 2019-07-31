using OPCAIC.Messaging.Config;

namespace OPCAIC.Worker.Config
{
	/// <summary>
	///     Configuration root for the Worker executable.
	/// </summary>
	public class Configuration
	{
		/// <summary>
		///     Configuration of the broker connection.
		/// </summary>
		public WorkerConnectorConfig ConnectorConfig { get; set; }

		/// <summary>
		///     Configuration of the connection to the file server.
		/// </summary>
		public FileServerConfig FileServer { get; set; }

		/// <summary>
		///     Path where the game modules are stored.
		/// </summary>
		public string ModulePath { get; set; }

		/// <summary>
		///     Configuration of the task execution.
		/// </summary>
		public ExecutionConfig Execution { get; set; }
	}
}