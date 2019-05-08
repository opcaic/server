using System;
using System.Threading;
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
				using (var connector = new WorkerConnector(ConnectionString, identity.ToString(), HeartbeatConfig.Default))
				{
					connector.RegisterHandler<ExecuteMatchMessage>(msg =>
					{
						Console.WriteLine($"[{identity}] - received workload: {msg.Game}");
						Thread.Sleep(500 * rand.Next(4));
						if (rand.Next(10) == 0)
						{
							Console.WriteLine($"[{identity}] - simulating crash");
							connector.StopPoller();
						}

						connector.SendMessage(new MatchExecutionResultMessage(1));
					});

					Thread t = new Thread(() => connector.EnterConsumer());
					t.Start();
					Console.WriteLine($"[{identity}] - Initiating connection");
					connector.SendMessage(new WorkerConnectMessage()
					{
						Capabilities = new WorkerCapabilities()
						{
							SupportedGames = {"game1"},
							SupportedLanguages = { "dotnet", "cpp" }
						},
					});
					connector.EnterPoller(); // returns on worker exit
					connector.StopConsumer();
					t.Join();
				}
				Console.WriteLine($"[{identity}] - client officially dead, restarting in 5s");
				Thread.Sleep(5000);
			}
		}
	}
}