using NetMQ.Sockets;

namespace OPCAIC.Messaging
{
	public class DealerSocketFactory : SocketFactory<DealerSocket>
	{
		public DealerSocketFactory(string identity, string address, bool bind) : base(identity, address,
			bind)
		{
		}

		public override DealerSocket CreateRawSocket() => new DealerSocket();
	}
}