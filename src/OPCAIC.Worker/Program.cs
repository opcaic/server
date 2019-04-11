using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using OPCAIC.Messaging;

namespace OPCAIC.Worker
{
	internal class Program
	{
		public static void Main(string[] args)
		{
			if (args.Length != 2 || !int.TryParse(args[1], out var port))
			{
				Console.WriteLine("usage: [server] [port]");
				return;
			}

			var connector = new ClientConnector($"tcp://localhost:{port}", "client1");
			Thread t = new Thread(()=>connector.EnterConsumer());
			connector.SendMessage(new WorkerConnectMessage("HELLO"));
			connector.EnterPoller();

		}
	}
}
