using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace OPCAIC.Messaging.Test
{
	public static class TestConnectionStringFactory
	{
		// no seed! we want multiple processes to get different ports
		private static int NextPort = new Random().Next(5000, 8000);

		public static string GetConnectionString()
		{
			while (true)
			{
				var port = Interlocked.Increment(ref NextPort);
				try
				{
					var listener = new TcpListener(IPAddress.Loopback, port);
					listener.Start();
					listener.Stop();
					return $"tcp://localhost:{port}"; // this port is free
				}
				catch
				{
					// try again
				}
			}
		}
	}
}