using System;
using Microsoft.Extensions.Logging.Abstractions;
using OPCAIC.Messaging.Config;
using Xunit;
using Xunit.Abstractions;


namespace OPCAIC.Messaging.Test
{
	public class BrokerWorkerTestBase : IDisposable
	{
		protected readonly ITestOutputHelper Output;
		public string ConnectionString = TestConnectionStringFactory.GetConnectionString();

		protected static readonly double Timeout = 5000;

		protected BrokerConnector Broker { get; private set; }
		protected WorkerConnector Worker { get; private set; }
		protected HeartbeatConfig Config { get; }

		public BrokerWorkerTestBase(ITestOutputHelper output)
		{
			Output = output;
			Output.WriteLine(ConnectionString);
			Config = HeartbeatConfig.Default;
		}

		protected void CreateBroker()
		{
			KillBroker();
			Broker = new BrokerConnector(new BrokerConnectorConfig()
			{
				Identity = "Broker",
				ListeningAddress = ConnectionString,
				HeartbeatConfig = Config
			}, new NullLogger<BrokerConnector>());
		}

		protected void CreateWorker()
		{
			KillWorker();
			Worker = new WorkerConnector(new WorkerConnectorConfig()
			{
				Identity = "Worker",
				BrokerAddress = ConnectionString,
				HeartbeatConfig = Config
			}, new NullLogger<WorkerConnector>());
		}

		protected void CreateConnectors()
		{
			CreateBroker();
			CreateWorker();
		}

		protected void StartAll()
		{
			StartBroker();
			StartWorker();
		}

		protected void StartWorker()
		{
			if (Worker == null) CreateWorker();
			Worker.EnterPollerAsync();
			Worker.EnterConsumerAsync();
		}

		protected void StartBroker()
		{
			if (Broker == null) CreateBroker();
			Broker.EnterPollerAsync();
			Broker.EnterConsumerAsync();
		}

		protected void StopAll()
		{
			StopBroker();
			StopWorker();
		}

		protected void StopWorker()
		{
			try { Worker.StopPoller(); }
			catch { /*ignore*/ }
			try { Worker.StopConsumer(); }
			catch { /*ignore*/ }
		}

		protected void StopBroker()
		{
			try { Broker.StopPoller(); }
			catch { /*ignore*/ }
			try { Broker.StopConsumer(); }
			catch { /*ignore*/ }
		}

		protected void KillAll()
		{
			KillBroker();
			KillWorker();
		}

		protected void KillWorker()
		{
			if (Worker != null)
			{
				StopWorker();
				Worker.Dispose();
			}
			Worker = null;
		}

		protected void KillBroker()
		{
			if (Broker != null)
			{
				StopBroker();
				Broker.Dispose();
			}
			Broker = null;
		}

		public void Dispose()
		{
			KillAll();
		}
	}
}