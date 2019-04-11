using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using OPCAIC.Messaging;
using OPCAIC.Messaging.Messages;

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

			string identity = "client";

			var connector = new WorkerConnector($"tcp://localhost:{port}", identity);
			connector.RegisterHandler<WorkLoadMessage>(msg =>
			{
				Console.WriteLine($"[{identity}] - received workload: {msg.Work}");
				Thread.Sleep(500);
				connector.SendMessage(new WorkCompletedMessage(msg.Work));
			});

			Thread t = new Thread(()=>connector.EnterConsumer());
			t.Start();
			connector.SendMessage(new WorkerConnectMessage("HELLO"));
			connector.EnterPoller();

		}
	}
}
