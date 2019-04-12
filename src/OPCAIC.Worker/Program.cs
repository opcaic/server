using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;

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

			WorkerProcess.ConnectionString = $"tcp://localhost:{port}";

			Random rand = new Random();
			string identity = $"client{rand.Next(100)}";
			WorkerProcess.Start(identity);
		}
	}
}
