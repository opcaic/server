namespace OPCAIC.Messaging
{
	/// <summary>
	///     Args for events concerning the worker connection.
	/// </summary>
	public class WorkerConnectionEventArgs
	{
		/// <summary>
		///     Identity of the worker.
		/// </summary>
		public string Identity { get; set; }
	}
}