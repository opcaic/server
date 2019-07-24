using System;
using System.Diagnostics;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OPCAIC.Messaging.Config;
using OPCAIC.TestUtils;
using Xunit.Abstractions;

namespace OPCAIC.Messaging.Test
{
	public class BrokerWorkerConnectorTestBase : IDisposable
	{
		protected static readonly double Timeout = 5000;

		private readonly XUnitLoggerProvider loggerProvider;
		public string ConnectionString = TestConnectionStringFactory.GetConnectionString();

		public BrokerWorkerConnectorTestBase(ITestOutputHelper output)
		{
			output.WriteLine(ConnectionString);
			loggerProvider = new XUnitLoggerProvider(output);
			HeartbeatConfig = HeartbeatConfig.Default;
			BrokerConsumerThread = new ThreadHelper(output, "BrokerConsumer");
			BrokerSocketThread = new ThreadHelper(output, "BrokerSocket");
			WorkerConsumerThread = new ThreadHelper(output, "WorkerSocket");
			WorkerSocketThread = new ThreadHelper(output, "WorkerSocket");
		}

		protected BrokerConnector Broker { get; private set; }
		private ThreadHelper BrokerConsumerThread { get; }
		private ThreadHelper BrokerSocketThread { get; }
		protected WorkerConnector Worker { get; private set; }
		private ThreadHelper WorkerConsumerThread { get; }
		private ThreadHelper WorkerSocketThread { get; }
		protected HeartbeatConfig HeartbeatConfig { get; }

		public void Dispose() => KillAll();

		private ILogger<T> GetLogger<T>() => loggerProvider.CreateLogger<T>();

		protected void CreateBroker()
		{
			KillBroker();
			Broker = new BrokerConnector(Options.Create(new BrokerConnectorConfig
			{
				Identity = "Broker",
				ListeningAddress = ConnectionString,
				HeartbeatConfig = HeartbeatConfig
			}), GetLogger<BrokerConnector>());
		}

		protected void CreateWorker()
		{
			KillWorker();
			Worker = new WorkerConnector(Options.Create(new WorkerConnectorConfig
			{
				Identity = "Worker",
				BrokerAddress = ConnectionString,
				HeartbeatConfig = HeartbeatConfig
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

			Debug.Assert(!WorkerSocketThread.IsRunning);
			Debug.Assert(!WorkerConsumerThread.IsRunning);

			WorkerSocketThread.Start(Worker.EnterSocket, Worker.StopSocket);
			WorkerConsumerThread.Start(Worker.EnterConsumer, Worker.StopConsumer);
		}

		protected void StartBroker()
		{
			if (Broker == null)
			{
				CreateBroker();
			}

			Debug.Assert(!BrokerSocketThread.IsRunning);
			Debug.Assert(!BrokerConsumerThread.IsRunning);

			BrokerSocketThread.Start(Broker.EnterSocket, Broker.StopSocket);
			BrokerConsumerThread.Start(Broker.EnterConsumer, Broker.StopConsumer);
		}

		protected void StopAll()
		{
			StopBroker();
			StopWorker();
		}

		protected void StopWorker()
		{
			if (WorkerSocketThread.IsRunning)
			{
				WorkerSocketThread.Stop();
			}

			if (WorkerConsumerThread.IsRunning)
			{
				WorkerConsumerThread.Stop();
			}
		}

		protected void StopBroker()
		{
			if (BrokerSocketThread.IsRunning)
			{
				BrokerSocketThread.Stop();
			}

			if (BrokerConsumerThread.IsRunning)
			{
				BrokerConsumerThread.Stop();
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
