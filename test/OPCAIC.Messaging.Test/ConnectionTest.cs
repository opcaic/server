using System.Threading;
using OPCAIC.Messaging.Messages;
using Xunit;
using Xunit.Abstractions;

namespace OPCAIC.Messaging.Test
{
	public class ConnectionTest : BrokerWorkerTestBase
	{
		[Fact]
		public void WorkerDeadAfterTimeout()
		{
			ManualResetEventSlim flag = new ManualResetEventSlim(false);
			Broker.WorkerConnected += (_, a) => StopWorker();
			string workerId = null;
			Broker.WorkerDisconnected += (_, a) =>
			{
				workerId = a.Identity;
				flag.Set();
			};

			StartAll();
			Worker.SendMessage(new PingMessage());

			AssertEx.WaitForEvent(flag, Timeout);
			Assert.Equal(Worker.Identity, workerId);
		}

		[Fact]
		public void WorkerReconnectsToBroker()
		{
			ManualResetEventSlim connFlag = new ManualResetEventSlim(false);
			ManualResetEventSlim disconFlag = new ManualResetEventSlim(false);
			Worker.Connected += (_, a) => connFlag.Set();
			Worker.Disconnected += (_, a) => disconFlag.Set();

			StartAll();
			Worker.SendMessage(new PingMessage());

			AssertEx.WaitForEvent(connFlag, Timeout);
			KillBroker();
			connFlag.Reset();
			AssertEx.WaitForEvent(disconFlag, Timeout);
			StartBroker();

			AssertEx.WaitForEvent(connFlag, Timeout);
		}

		public ConnectionTest(ITestOutputHelper output) : base(output)
		{
			Config.HeartbeatInterval = 10;
			Config.ReconnectIntervalInit = 10;
			CreateConnectors();
		}
	}
}