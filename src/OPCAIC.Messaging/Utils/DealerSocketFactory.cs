using NetMQ.Sockets;

namespace OPCAIC.Messaging.Utils
{
	public class DealerSocketFactory : SocketFactory<DealerSocket>
	{
		public DealerSocketFactory(string identity, string address) : base(identity, address,
			false)
		{
		}

		public override DealerSocket CreateRawSocket() => new DealerSocket();
	}
}
