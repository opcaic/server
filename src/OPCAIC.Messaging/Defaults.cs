namespace OPCAIC.Messaging
{
	public static class Defaults
	{
		public static int HeartbeatInterval => 1000;
		public static int Liveness => 3;
		public static int ReconnectIntervalInit => 1000;
		public static int ReconnectIntervalMax => 32000;
	}
}