﻿using NetMQ;

namespace OPCAIC.Messaging.Utils
{
	public interface ISocketFactory<TSocket> where TSocket : NetMQSocket
	{
		string Identity { get; }
		TSocket CreateSocket();
	}
}