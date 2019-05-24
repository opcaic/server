using System;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using OPCAIC.Messaging.Messages;
using OPCAIC.Messaging.Test.Messages;
using Xunit;
using Xunit.Abstractions;
using Xunit.Sdk;

namespace OPCAIC.Messaging.Test
{
	public class SimpleTest : BrokerWorkerTestBase
	{
		[Fact]
		public void WorkerConnects()
		{
			ManualResetEventSlim flag = new ManualResetEventSlim(false);
			Broker.WorkerConnected += (_, a) => flag.Set();

			StartAll();

			AssertEx.WaitForEvent(flag, Timeout);
		}

		[Fact]
		public void BrokerConnects()
		{
			ManualResetEventSlim flag = new ManualResetEventSlim(false);
			Worker.Connected += (_, a) => flag.Set();

			StartAll();

			AssertEx.WaitForEvent(flag, Timeout);
		}

		[Fact]
		public void SimpleSendMessage()
		{
			ManualResetEventSlim workerReceive = new ManualResetEventSlim(false);
			ManualResetEventSlim brokerReceive = new ManualResetEventSlim(false);
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
			Broker.SendMessage(Worker.Identity, new HelloMessage
			{
				Message = "message"
			});

			// TODO: if not signaled, then some thread may have crashed due to assert
			AssertEx.WaitForEvent(workerReceive, Timeout);
			AssertEx.WaitForEvent(brokerReceive, Timeout);
		}

		public SimpleTest(ITestOutputHelper output) : base(output)
		{
			// no extra setup needed
			CreateConnectors();
		}
	}
}
