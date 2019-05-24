using System;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OPCAIC.Messaging.Config;
using OPCAIC.TestUtils;
using Xunit.Abstractions;

namespace OPCAIC.Messaging.Test
{
	public class BrokerWorkerTestBase : IDisposable
	{
		protected static readonly double Timeout = 5000;

		private readonly XUnitLoggerFactory loggerFactory;
		public string ConnectionString = TestConnectionStringFactory.GetConnectionString();

		public BrokerWorkerTestBase(ITestOutputHelper output)
		{
			output.WriteLine(ConnectionString);
			loggerFactory = new XUnitLoggerFactory(output);
			Config = HeartbeatConfig.Default;
		}

		protected BrokerConnector Broker { get; private set; }
		protected WorkerConnector Worker { get; private set; }
		protected HeartbeatConfig Config { get; }

		public void Dispose() => KillAll();

		private ILogger<T> GetLogger<T>() => loggerFactory.CreateLogger<T>();

		protected void CreateBroker()
		{
			KillBroker();
			Broker = new BrokerConnector(Options.Create(new BrokerConnectorConfig
			{
				Identity = "Broker",
				ListeningAddress = ConnectionString,
				HeartbeatConfig = Config
			}), GetLogger<BrokerConnector>());
		}

		protected void CreateWorker()
		{
			KillWorker();
			Worker = new WorkerConnector(Options.Create(new WorkerConnectorConfig
			{
				Identity = "Worker",
				BrokerAddress = ConnectionString,
				HeartbeatConfig = Config
			}), GetLogger<WorkerConnector>());
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
			if (Worker == null)
			{
				CreateWorker();
			}

			Worker.EnterPollerAsync();
			Worker.EnterConsumerAsync();
		}

		protected void StartBroker()
		{
			if (Broker == null)
			{
				CreateBroker();
			}

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
			try
			{
				Worker.StopPoller();
			}
			catch
			{ /*ignore*/
			}

			try
			{
				Worker.StopConsumer();
			}
			catch
			{ /*ignore*/
			}
		}

		protected void StopBroker()
		{
			try
			{
				Broker.StopPoller();
			}
			catch
			{ /*ignore*/
			}

			try
			{
				Broker.StopConsumer();
			}
			catch
			{ /*ignore*/
			}
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
	}
}
