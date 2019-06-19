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
		///   User name to be used when connecting. If null, then no credentials are used
		/// </summary>
		public string UserName { get; set; }

		/// <summary>
		///   Password to be used when connecting. Used only if <see cref="UserName"/> is not null.
		/// </summary>
		public string Password { get; set; }
	}
}
