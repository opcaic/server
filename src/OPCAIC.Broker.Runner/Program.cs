using OPCAIC.Messaging;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using OPCAIC.Messaging.Messages;
using OPCAIC.Worker;

namespace OPCAIC.Broker.Runner
{
	internal class Program
	{
		private static int port;
		private static int counter;
		private const int workerCount = 1;

		private static bool stop;

		public static void StartBroker(string conectionString)
		{
			using (var broker = new Broker(new BrokerConnector(conectionString, "Broker", HeartbeatConfig.Default)))
			{
				broker.StartBrokering();
				while (!stop)
				{
					Thread.Sleep(1000);
				}
				broker.StopBrokering();
			}
		}

		public static int Main(string[] args)
		{
			if (args.Length != 2 || !int.TryParse(args[1], out port))
			{
				Console.WriteLine("Usage: [host] [port]");
				return 1;
			}

			WorkerProcess.ConnectionString = $"tcp://localhost:{port}";

			List<Thread> workers = new List<Thread>(workerCount);
			for (int i = 0; i < workerCount; i++)
			{
				workers.Add(new Thread(WorkerProcess.Start));
				workers[i].Start($"Client{i}");
			}

			Console.CancelKeyPress += (_, a) =>
			{
				a.Cancel = true;
				stop = true;
			};

			StartBroker(WorkerProcess.ConnectionString);

			return 0;
		}
	}
}
