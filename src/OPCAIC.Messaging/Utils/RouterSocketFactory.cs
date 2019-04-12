using NetMQ.Sockets;

namespace OPCAIC.Messaging.Utils
{
	public class RouterSocketFactory : SocketFactory<RouterSocket>
	{
		public RouterSocketFactory(string identity, string address, bool bind) : base(identity, address,
			bind)
		{
		}

		public override RouterSocket CreateRawSocket() => new RouterSocket();
	}
}