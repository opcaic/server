using System.Threading;
using Xunit;
using Xunit.Abstractions;

namespace OPCAIC.Messaging.Test
{
	public class ConnectionTest : BrokerWorkerTestBase
	{
		public ConnectionTest(ITestOutputHelper output) : base(output)
		{
			Config.HeartbeatInterval = 10;
			Config.ReconnectIntervalInit = 10;
			CreateConnectors();
		}

		[Fact]
		public void WorkerDeadAfterTimeout()
		{
			var flag = new ManualResetEventSlim(false);
			Broker.WorkerConnected += (_, a) => StopWorker();
			string workerId = null;
			Broker.WorkerDisconnected += (_, a) =>
			{
				workerId = a.Identity;
				flag.Set();
			};

			StartAll();

			AssertEx.WaitForEvent(flag, Timeout);
			Assert.Equal(Worker.Identity, workerId);
		}

		[Fact]
		public void WorkerReconnectsToBroker()
		{
			var connFlag = new ManualResetEventSlim(false);
			var disconFlag = new ManualResetEventSlim(false);
			Worker.Connected += (_, a) => connFlag.Set();
			Worker.Disconnected += (_, a) => disconFlag.Set();

			StartAll();

			AssertEx.WaitForEvent(connFlag, Timeout);
			KillBroker();
			connFlag.Reset();
			AssertEx.WaitForEvent(disconFlag, Timeout);
			StartBroker();

			AssertEx.WaitForEvent(connFlag, Timeout);
		}
	}
}
