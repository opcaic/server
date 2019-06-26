using System;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using OPCAIC.Messaging;
using OPCAIC.Messaging.Messages;
using OPCAIC.Worker.GameModules;
using OPCAIC.Worker.Services;
using Xunit;
using Xunit.Abstractions;

namespace OPCAIC.Worker.Test
{
	public class WorkerTest : WorkerTestBase
	{
		public WorkerTest(ITestOutputHelper output) : base(output) 
			=> Services
			.AddTransient<IGameModuleRegistry, GameModuleRegistry>();

		private void RunWorker() => GetService<Worker>().Run();

		[Theory]
		[MemberData(nameof(RequestTestData))]
		public void RespondsToMessages<TRequest, TResponse>(TRequest request, TResponse response)
			where TRequest : WorkMessageBase, new() where TResponse : ReplyMessageBase, new()
		{
			var jobMock = Services.Mock<IJobExecutor<TRequest, TResponse>>();
			var connectorMock = SetupConnectorReceive(request);

			// setup response from job executor
			jobMock.Setup(j => j.Execute(request)).Returns(response);

			RunWorker();

			// check that the respons was actually sent
			connectorMock.Verify(c => c.SendMessage(response));
			jobMock.VerifyAll();
		}

		private Mock<IWorkerConnector> SetupConnectorReceive<TRequest>(TRequest request)
		{
			Action<TRequest> handler = null;
			var connectorMock = Services.Mock<IWorkerConnector>();
			// capture the handler
			connectorMock.Setup(c => c.RegisterAsyncHandler(It.IsAny<Action<TRequest>>()))
				.Callback((Action<TRequest> h) => { handler = h; });

			// run the handler after entering consumer poller
			connectorMock.Setup(c => c.EnterConsumer()).Callback(() => { handler(request); });
			return connectorMock;
		}

		[Theory]
		[MemberData(nameof(RequestTestData))]
		public void ErrorStatusOnException<TRequest, TResponse>(TRequest request, TResponse _)
			where TRequest : WorkMessageBase, new() where TResponse : ReplyMessageBase, new()
		{
			var jobMock = Services.Mock<IJobExecutor<TRequest, TResponse>>();
			var connectorMock = SetupConnectorReceive(request);

			// simulate an error
			jobMock.Setup(j => j.Execute(It.IsAny<TRequest>())).Throws<Exception>();

			RunWorker();

			// check that a response was actually sent
			connectorMock.Verify(c => c.SendMessage(It.Is<TResponse>(r => r.Status == Status.Error)));
			jobMock.VerifyAll();
		}

		[Theory]
		[MemberData(nameof(RequestTestData))]
		public void NeverSendsNull<TRequest, TResponse>(TRequest request, TResponse _)
			where TRequest : WorkMessageBase, new() where TResponse : ReplyMessageBase, new()
		{
			var jobMock = Services.Mock<IJobExecutor<TRequest, TResponse>>();
			var connectorMock = SetupConnectorReceive(request);

			// simulate an error
			jobMock.Setup(j => j.Execute(It.IsAny<TRequest>())).Returns((TResponse) null);

			RunWorker();

			// check that a response was actually sent
			connectorMock.Verify(c => c.SendMessage(It.Is<TResponse>(r => r.Status == Status.Error)));
			jobMock.VerifyAll();
		}

		[Fact]
		public void StartupTest()
		{
			var connectorMock = Services.Mock<IWorkerConnector>();

			RunWorker();

			connectorMock.Verify(c => c.SendMessage(It.IsAny<WorkerConnectMessage>()));
			connectorMock.Verify(c => c.EnterConsumer());
			connectorMock.Verify(c => c.EnterSocket());
		}

		[Fact]
		public void UpdatesHeartbeatConfig()
		{
			var connectorMock = Services.Mock<IWorkerConnector>();

			RunWorker();

			connectorMock.Verify(c => c.SendMessage(It.IsAny<WorkerConnectMessage>()));
			connectorMock.Verify(c => c.EnterConsumer());
			connectorMock.Verify(c => c.EnterSocket());
		}

		public static TheoryData<WorkMessageBase, ReplyMessageBase> RequestTestData()
		{
			var ret = new TheoryData<WorkMessageBase, ReplyMessageBase>();
			ret.Add(new MatchExecutionRequest(), new MatchExecutionResult());
			ret.Add(new SubmissionValidationRequest(), new SubmissionValidationResult());

			return ret;
		}
	}
}
