namespace OPCAIC.Messaging.Config
{
	/// <summary>
	///   Configuration of the network connection on the worker node.
	/// </summary>
	public class WorkerConnectorConfig
	{
		/// <summary>
		///   Identity used in messages.
		/// </summary>
		public string Identity { get; set; }

		/// <summary>
		///   Broker address, to which to connect. The address must include a protocol and a port, e.g.
		///   "tcp://localhost:5000".
		/// </summary>
		public string BrokerAddress { get; set; }

		/// <summary>
		///   Configuration of the heartbeat messages.
		/// </summary>
		public HeartbeatConfig HeartbeatConfig { get; set; }
	}
}
