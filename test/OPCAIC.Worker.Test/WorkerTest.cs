﻿using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.VisualStudio.TestPlatform.Common.Utilities;
using Moq;
using OPCAIC.Messaging;
using OPCAIC.Messaging.Config;
using OPCAIC.Messaging.Messages;
using OPCAIC.TestUtils;
using OPCAIC.Worker.Config;
using OPCAIC.Worker.GameModules;
using OPCAIC.Worker.Services;
using Xunit;
using Xunit.Abstractions;

namespace OPCAIC.Worker.Test
{
	public class WorkerTest : ServiceTestBase
	{
		private readonly WorkerConnectorHelper connectorHelper;
		private Mock<IJobExecutor<MatchExecutionRequest, MatchExecutionResult>> jobMock;

		public WorkerTest(ITestOutputHelper output) : base(output)
		{
			Services
				.Configure<ExecutionConfig>(cfg =>
				{
					cfg.MaxTaskTimeout = 1000;
				});
			Services.Mock<IGameModuleRegistry>();

			jobMock = Services.Mock<IJobExecutor<MatchExecutionRequest, MatchExecutionResult>>();

			connectorHelper = new WorkerConnectorHelper(Services.Mock<IWorkerConnector>());
		}

		private void RunWorker() => GetService<Worker>().Run();

		[Fact]
		public void RespondsToMessages()
		{
			var request = new MatchExecutionRequest();
			var response = new MatchExecutionResult();
			connectorHelper.SetupConsumerReceive(request);

			// setup response from job executor
			jobMock.Setup(j => j.ExecuteAsync(request, It.IsAny<CancellationToken>())).ReturnsAsync(response);

			RunWorker();

			// check that the response was actually sent
			connectorHelper.Mock.Verify(c => c.SendMessage(response));
			jobMock.VerifyAll();
		}

		[Fact]
		public void ErrorStatusOnException()
		{
			var request = new MatchExecutionRequest();
			connectorHelper.SetupConsumerReceive(request);

			// simulate an error
			jobMock.Setup(j => j.ExecuteAsync(It.IsAny<MatchExecutionRequest>(), It.IsAny<CancellationToken>()))
				.Throws<Exception>();

			RunWorker();

			// check that a response was actually sent
			connectorHelper.Mock.Verify(c => c.SendMessage(It.Is<MatchExecutionResult>(r => r.JobStatus == JobStatus.Error)));
			jobMock.VerifyAll();
		}

		[Fact]
		public void NeverSendsNull()
		{
			var request = new MatchExecutionRequest();
			connectorHelper.SetupConsumerReceive(request);

			// simulate an error
			jobMock.Setup(j => j.ExecuteAsync(It.IsAny<MatchExecutionRequest>(), It.IsAny<CancellationToken>()))
				.ReturnsAsync((MatchExecutionResult) null);

			RunWorker();

			// check that a response was actually sent
			connectorHelper.Mock.Verify(c => c.SendMessage(It.Is<MatchExecutionResult>(r => r.JobStatus == JobStatus.Error)));
			jobMock.VerifyAll();
		}

		[Fact]
		public void StartupTest()
		{
			RunWorker();

			connectorHelper.Mock.Verify(c => c.SendMessage(It.IsAny<WorkerConnectMessage>()));
			connectorHelper.Mock.Verify(c => c.EnterConsumer());
			connectorHelper.Mock.Verify(c => c.EnterSocket());
		}

		[Fact]
		public void UpdatesHeartbeatConfig()
		{
			var config = new HeartbeatConfig();
			connectorHelper.SetupSocketReceive(new SetHeartbeatMessage() { HeartbeatConfig = config });

			RunWorker();

			connectorHelper.Mock.Verify(c => c.SetHeartbeatConfig(config));
		}

		[Fact]
		public void TaskTimeoutsAfterInterval()
		{
			// adjust timeout
			Services.Configure<ExecutionConfig>(cfg =>
			{
				cfg.MaxTaskTimeout = 10; // short timeout
			});

			DoExecuteCancel(false);

			connectorHelper.Mock.Verify(c => c.SendMessage(It.Is<MatchExecutionResult>(r => r.JobStatus == JobStatus.Timeout)));
			connectorHelper.Mock.VerifyAll();
			jobMock.VerifyAll();
		}

		private void DoExecuteCancel(bool externalCancel)
		{
			var request = new MatchExecutionRequest();
			var workStarted = new ManualResetEventSlim(false);

			connectorHelper.SetupConsumerReceive(request);
			connectorHelper.SetupSocket(() => workStarted.Wait());

			if (externalCancel) // external cancel by message
			{
				connectorHelper.SetupSocketReceive(new CancelTaskMessage());
			}

			jobMock.Setup(j => j.ExecuteAsync(request, It.IsAny<CancellationToken>()))
				.Callback((MatchExecutionRequest req, CancellationToken token) =>
				{
					workStarted.Set();
					// wait for the actual cancellation request
					AssertEx.WaitForEvent(() => token.IsCancellationRequested,
						TimeSpan.FromMilliseconds(200));
				}).ReturnsAsync(new MatchExecutionResult {JobStatus = JobStatus.Timeout});

			RunWorker();
		}

		[Fact]
		public void TaskCanceledByMessage()
		{
			// adjust timeout
			Services.Configure<ExecutionConfig>(cfg =>
			{
				cfg.MaxTaskTimeout = 1000; // long timeout
			});

			DoExecuteCancel(true);

			connectorHelper.Mock.VerifyAll();
			jobMock.VerifyAll();
		}
	}
}
