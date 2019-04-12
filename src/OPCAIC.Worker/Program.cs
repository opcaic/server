using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using OPCAIC.Messaging;
using OPCAIC.Messaging.Messages;

namespace OPCAIC.Worker
{
	public class WorkerProcess
	{
		public static string ConnectionString = "tcp://localhost:5000"; // for test purposes
		public static void Start(object identity)
		{
			Random rand = new Random();
			while (true)
			{
				using (var connector = new WorkerConnector(ConnectionString, identity.ToString()))
				{
					connector.RegisterHandler<WorkLoadMessage>(msg =>
					{
						Console.WriteLine($"[{identity}] - received workload: {msg.Work}");
						Thread.Sleep(500 * rand.Next(4));
						if (rand.Next(10) == 0)
						{
							Console.WriteLine($"[{identity}] - simulating crash");
							connector.StopPoller();
						}

						connector.SendMessage(new WorkCompletedMessage(msg.Work));
					});

					Thread t = new Thread(() => connector.EnterConsumer());
					t.Start();
					Console.WriteLine($"[{identity}] - Initiating connection");
					connector.SendMessage(new WorkerConnectMessage("HELLO"));
					connector.EnterPoller(); // returns on worker exit
					connector.StopConsumer();
					t.Join();
				}
				Console.WriteLine($"[{identity}] - client officially dead, restarting in 5s");
				Thread.Sleep(5000);
			}
		}
	}

	internal class Program
	{
		public static void Main(string[] args)
		{
			if (args.Length != 2 || !int.TryParse(args[1], out var port))
			{
				Console.WriteLine("usage: [server] [port]");
				return;
			}

			WorkerProcess.ConnectionString = $"tcp://localhost:{port}";

			Random rand = new Random();
			string identity = $"client{rand.Next(100)}";
			WorkerProcess.Start(identity);
		}
	}
}
