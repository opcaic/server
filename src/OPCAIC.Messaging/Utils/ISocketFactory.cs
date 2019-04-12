using NetMQ;

namespace OPCAIC.Messaging.Utils
{
	public interface ISocketFactory<TSocket> where TSocket : NetMQSocket
	{
		TSocket CreateSocket();
	}
}