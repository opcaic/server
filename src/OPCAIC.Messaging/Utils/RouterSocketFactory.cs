using NetMQ.Sockets;

namespace OPCAIC.Messaging.Utils
{
	public class RouterSocketFactory : SocketFactory<RouterSocket>
	{
		public RouterSocketFactory(string identity, string address) : base(identity, address,
			true)
		{
		}

		public override RouterSocket CreateRawSocket() => new RouterSocket();
	}
}
