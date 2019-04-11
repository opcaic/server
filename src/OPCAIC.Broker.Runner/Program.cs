using OPCAIC.Messaging;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using OPCAIC.Messaging.Messages;

namespace OPCAIC.Broker.Runner
{
	internal class Program
	{
		private static int port;
		private static int counter;
		private const int workerCount = 0;
		public static int Main(string[] args)
		{
			if (args.Length != 2 || !int.TryParse(args[1], out port))
			{
				Console.WriteLine("Usage: [host] [port]");
				return 1;
			}

			List<Thread> workers = new List<Thread>(workerCount);
			for (int i = 0; i < workerCount; i++)
			{
				workers.Add(new Thread(Worker));
				workers[i].Start($"Client{i}");
			}

			BrokerConnector connector = new BrokerConnector($"tcp://localhost:{port}", "Broker");
			connector.RegisterHandler<WorkerConnectMessage>((s, msg) =>
			{
				Console.WriteLine($"[Broker] - received: {msg.Message}");
				connector.SendMessage(s, new WorkLoadMessage(counter++));
			});
			connector.RegisterHandler<WorkCompletedMessage>((s, msg) =>
			{
				Console.WriteLine($"[Broker] - received completion report: {msg.Work}");
				connector.SendMessage(s, new WorkLoadMessage(counter++));
			});
			
			Thread t = new Thread(()=>connector.EnterConsumer());
			t.Start();
			connector.EnterPoller();

			return 0;
		}


		public static void Worker(object identity)
		{
			var connector = new WorkerConnector($"tcp://localhost:{port}", identity.ToString());
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
