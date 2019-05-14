namespace OPCAIC.Messaging.Config
{
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

		public int HeartbeatInterval { get; set; }
		public int Liveness { get; set; }
		public int ReconnectIntervalInit { get; set; }
		public int ReconnectIntervalMax { get; set; }
	}
}