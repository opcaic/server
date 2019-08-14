using System.Threading;
using OPCAIC.Messaging.Test.Messages;
using OPCAIC.TestUtils;
using Xunit;
using Xunit.Abstractions;

namespace OPCAIC.Messaging.Test
{
	public class SimpleConnectorTest : BrokerWorkerConnectorTestBase
	{
		public SimpleConnectorTest(ITestOutputHelper output) : base(output)
		{
			CreateConnectors();
		}

		[Fact]
		public void BrokerConnects()
		{
			var flag = new ManualResetEventSlim(false);
			Worker.Connected += (_, a) => flag.Set();

			StartAll();

			AssertEx.WaitForEvent(flag, Timeout);
		}

		[Fact]
		public void SimpleSendMessage()
		{
			var workerReceive = new ManualResetEventSlim(false);
			var brokerReceive = new ManualResetEventSlim(false);
			Worker.RegisterAsyncHandler<HelloMessage>(msg =>
			{
				Assert.Equal("message", msg.Message);
				// send reply
				msg.Message = "reply";
				Worker.SendMessage(msg);
				workerReceive.Set();
			});
			Broker.RegisterAsyncHandler<HelloMessage>((sender, msg) =>
			{
				Assert.Equal(Worker.Identity, sender);
				Assert.Equal("reply", msg.Message);
				brokerReceive.Set();
			});

			StartAll();
			Broker.SendMessage(Worker.Identity, new HelloMessage {Message = "message"});

			// TODO: if not signaled, then some thread may have crashed due to assert
			AssertEx.WaitForEvent(workerReceive, Timeout);
			AssertEx.WaitForEvent(brokerReceive, Timeout);
		}

		[Fact]
		public void WorkerConnects()
		{
			var flag = new ManualResetEventSlim(false);
			Broker.WorkerConnected += (_, a) => flag.Set();

			StartAll();

			AssertEx.WaitForEvent(flag, Timeout);
		}
	}
}