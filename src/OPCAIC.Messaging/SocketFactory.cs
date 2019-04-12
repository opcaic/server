using System.Text;
using NetMQ;

namespace OPCAIC.Messaging
{
	public abstract class SocketFactory<TSocket> : ISocketFactory<TSocket>
		where TSocket : NetMQSocket
	{
		private readonly string address;
		private readonly bool bind;
		private readonly string identity;

		public SocketFactory(string identity, string address, bool bind)
		{
			this.identity = identity;
			this.address = address;
			this.bind = bind;
		}

		public TSocket CreateSocket()
		{
			var sock = CreateRawSocket();
			sock.Options.Identity = Encoding.Unicode.GetBytes(identity);
			if (bind)
			{
				sock.Bind(address);
			}
			else
			{
				sock.Connect(address);
			}

			return sock;
		}

		public abstract TSocket CreateRawSocket();
	}
}
