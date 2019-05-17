using NetMQ;

namespace OPCAIC.Messaging.Utils
{
	public abstract class SocketFactory<TSocket> : ISocketFactory<TSocket>
		where TSocket : NetMQSocket
	{
		private readonly string address;
		private readonly bool bind;

		public SocketFactory(string identity, string address, bool bind)
		{
			Identity = identity;
			this.address = address;
			this.bind = bind;
		}

		public string Identity { get; }

		public TSocket CreateSocket()
		{
			var sock = CreateRawSocket();
			sock.Options.Identity = MessageHelpers.IdentityToBytes(Identity);
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
