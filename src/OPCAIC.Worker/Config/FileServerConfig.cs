namespace OPCAIC.Worker.Config
{
	/// <summary>
	///   Configuration for the file server connection
	/// </summary>
	public class FileServerConfig
	{
		/// <summary>
		///   Address of the file server.
		/// </summary>
		public string ServerAddress { get; set; }

		/// <summary>
		///   Username to be used when connecting. If null, then no credentials are used
		/// </summary>
		public string Username { get; set; }

		/// <summary>
		///   Password to be used when connecting.
		/// </summary>
		public string Password { get; set; }
	}
}
