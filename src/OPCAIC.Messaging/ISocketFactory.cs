using NetMQ;

namespace OPCAIC.Messaging
{
	public interface ISocketFactory<TSocket> where TSocket : NetMQSocket
	{
		TSocket CreateSocket();
	}
}