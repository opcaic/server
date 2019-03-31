using System;
using System.Threading;
using System.Threading.Tasks;
using Grpc.Core;
using Grpc.Core.Logging;
using OPCAIC.Protos;

namespace OPCAIC.Broker.Runner
{
  internal class MasterImpl : Master.MasterBase
  {
    // Server side handler of the SayHello RPC
    public override async Task<HelloReply> SayHello(HelloRequest request, ServerCallContext context)
    {
      Console.WriteLine($"Received hello request from {request.ClientId}");
      //await Task.Delay(1000);
	    Thread.Sleep(1000); // simulate heavy load
      Console.WriteLine($"Servicing hello request from {request.ClientId}");
      return new HelloReply
      {
        Message = "Hello " + request.Name + " with id " + request.ClientId
      };
    }
  }

	internal class Program
	{
		public static int Main(string[] args)
		{
			if (args.Length != 2 || !int.TryParse(args[1], out int port))
			{
				Console.WriteLine("Usage: [host] [port]");
				return 1;
			}

			GrpcEnvironment.SetLogger(new ConsoleLogger());
			var server = new Server
			{
				Services = {Master.BindService(new MasterImpl())},
				Ports = {new ServerPort(args[0], port, ServerCredentials.Insecure)}
			};
			server.Start();

			Console.WriteLine($"Listening for incoming connections on {args[0]}:{port}");
			Console.WriteLine("Press any key to stop the server...");
			Console.ReadKey();

			server.ShutdownAsync().Wait();
			return 0;
		}
	}
}
