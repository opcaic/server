namespace OPCAIC.Messaging.Config
{
	/// <summary>
	///   Configuration of heartbeat messages between broker and worker
	/// </summary>
	public class HeartbeatConfig
	{
		public static HeartbeatConfig Default
			=> new HeartbeatConfig
			{
				HeartbeatInterval = 1000,
				Liveness = 3,
				ReconnectIntervalInit = 1000,
				ReconnectIntervalMax = 32000
			};

		/// <summary>
		///   Interval between heartbeats in milliseconds.
		/// </summary>
		public int HeartbeatInterval { get; set; }

		/// <summary>
		///   Maximum amount of missing heartbeats before node is considered dead.
		/// </summary>
		public int Liveness { get; set; }

		/// <summary>
		///   Minimum pause in milliseconds before worker tries to reconnect to broker after connection
		///   failure. If failure persist, try again after twice the interval, up to the
		///   <see
		///     cref="ReconnectIntervalMax" />
		///   .
		/// </summary>
		public int ReconnectIntervalInit { get; set; }

		/// <summary>
		///   Maximum pause in milliseconds before worker tries to reconnect to broker after connection
		///   failure.
		/// </summary>
		public int ReconnectIntervalMax { get; set; }
	}
}
