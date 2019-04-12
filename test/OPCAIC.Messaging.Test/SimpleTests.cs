using System;
using System.Diagnostics;
using System.Threading;
using OPCAIC.Messaging.Messages;
using OPCAIC.Messaging.Test.Messages;
using Xunit;

namespace OPCAIC.Messaging.Test
{
	public class SimpleTests : IDisposable
	{
		// TODO: dynamically create addresses in order for tests to run in parallel
		public static string ConnectionString = "tcp://localhost:5000"; // for test purposes

		private BrokerConnector brokerConnector;
		private WorkerConnector workerConnector;

		public SimpleTests()
		{
			brokerConnector = new BrokerConnector(ConnectionString, "Broker");
			workerConnector = new WorkerConnector(ConnectionString, "Worker");
		}

		private void StartAll()
		{
			brokerConnector.EnterPollerAsync();
			brokerConnector.EnterConsumerAsync();
			workerConnector.EnterPollerAsync();
			workerConnector.EnterConsumerAsync();
		}

		private void SetupMessages()
		{
		}

		[Fact]
		public void HelloTest()
		{
			workerConnector.RegisterHandler<WorkLoadMessage>(msg =>
			{
				Console.WriteLine($"[Worker] - received workload: {msg.WorkItem}");
				workerConnector.SendMessage(new WorkLoadCompleted { WorkItem = msg.WorkItem });
			});
			brokerConnector.RegisterAsyncHandler<WorkLoadCompleted>((_, msg) =>
			{
				Console.WriteLine($"[Broker] - received workload completed: {msg.WorkItem}");
			});

			bool flag = false;
			brokerConnector.RegisterAsyncHandler<HelloMessage>((_, msg) => { flag = true; });

			StartAll();
			workerConnector.SendMessage(new HelloMessage());
			Stopwatch sw = Stopwatch.StartNew();
			while (!flag && sw.Elapsed.TotalMilliseconds < 100) { }
			Assert.True(flag, "The broker did not receive message in given time");
		}

		public void Dispose()
		{
			brokerConnector.Dispose();
			workerConnector.Dispose();
		}
	}
}
