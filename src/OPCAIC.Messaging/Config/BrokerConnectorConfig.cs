namespace OPCAIC.Messaging.Config
{
	/// <summary>
	///     Configuration of the network connection on the broker node.
	/// </summary>
	public class BrokerConnectorConfig
	{
		/// <summary>
		///     Identity used in the messages.
		/// </summary>
		public string Identity { get; set; }

		/// <summary>
		///     Address on which to listen for workers. The address must include a protocol and a port, e.g.
		///     "tcp://localhost:5000".
		/// </summary>
		public string ListeningAddress { get; set; }

		/// <summary>
		///     Configuration of the heartbeat messages.
		/// </summary>
		public HeartbeatConfig HeartbeatConfig { get; set; }
	}
}