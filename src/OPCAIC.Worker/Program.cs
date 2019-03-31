using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Grpc.Core;
using Grpc.Core.Logging;
using OPCAIC.Protos;

namespace OPCAIC.Worker
{
	internal class Program
	{
		private static async Task DoRun(int i, string host, int port)
		{
			await Task.Delay(100);

			var channel = new Channel(host, port, ChannelCredentials.Insecure);

			Console.WriteLine($"Starting: {i}");
			var client = new Master.MasterClient(channel);
			var user = "you";

			var reply = await client.SayHelloAsync(new HelloRequest {Name = user, ClientId = i});
			Console.WriteLine($"Received: {reply}");

			await channel.ShutdownAsync();
		}

		public static void Main(string[] args)
		{
			if (args.Length != 2 || !int.TryParse(args[1], out var port))
			{
				Console.WriteLine("usage: [server] [port]");
				return;
			}

			GrpcEnvironment.SetLogger(new ConsoleLogger());

			// spawn multiple parallel connections, simulating multiple clients
			var tasks = new List<Task>();
			for (var i = 0; i < 10; i++)
				tasks.Add(DoRun(i, args[0], port));

			Task.WaitAll(tasks.ToArray());
		}
	}
}
