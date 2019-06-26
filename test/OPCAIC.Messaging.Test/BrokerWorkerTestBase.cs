using System;
using System.Diagnostics;
using System.Runtime.ExceptionServices;
using System.Threading;
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

		private ILogger<T> GetLogger<T>() => loggerFactory.CreateLogger<T>();

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

		private class ThreadHelper
		{
			private readonly string description;
			private readonly ITestOutputHelper output;
			private Exception ex;
			private Action stopAction;
			private Thread thread;

			public ThreadHelper(ITestOutputHelper output, string description)
			{
				this.output = output;
				this.description = description;
			}

			public bool IsRunning => thread != null;

			public void Start(Action enter, Action stop)
			{
				stopAction = stop;

				thread = new Thread(() =>
				{
					try
					{
						enter();
					}
					catch (Exception e)
					{
						output.WriteLine($"Thread '{description}' exited with exception: \n{e}");
						ex = e;
					}
				});

				thread.Name = description;
				thread.Start();
			}

			public void Stop()
			{
				stopAction();
				if (!thread.Join(100))
				{
					output.WriteLine("Thread Aborted");
					thread.Abort();
				}

				if (ex != null)
				{
					// rethrow any exception with original stack trace
					ExceptionDispatchInfo.Throw(ex); 
				}

				ex = null;
				thread = null;
				stopAction = null;
			}
		}
	}
}
