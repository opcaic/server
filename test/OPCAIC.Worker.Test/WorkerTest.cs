using System;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using OPCAIC.Messaging;
using OPCAIC.Messaging.Messages;
using OPCAIC.TestUtils;
using OPCAIC.Worker.GameModules;
using OPCAIC.Worker.Services;
using Xunit;
using Xunit.Abstractions;

namespace OPCAIC.Worker.Test
{
	public class WorkerTest
	{
		public WorkerTest(ITestOutputHelper output)
		{
			services = new MockingServiceCollection();
			services
				.AddXUnitLogging(output)
				.AddTransient<Worker>()
				.AddTransient<IGameModuleRegistry, GameModuleRegistry>();
		}

		private readonly MockingServiceCollection services;

		private Worker GetWorker() => services.BuildServiceProvider().GetRequiredService<Worker>();

		private void DoRespondsToMessages<TRequest, TResponse>()
			where TRequest : WorkMessageBase, new() where TResponse : ReplyMessageBase, new()
		{
			var jobMock = services.Mock<IJobExecutor<TRequest, TResponse>>();
			var request = new TRequest();
			var response = new TResponse();

			var connectorMock = SetupConnectorReceive(request);

			// setup response from job executor
			jobMock.Setup(j => j.Execute(request)).Returns(response);

			GetWorker().Run();

			// check that the respons was actually sent
			connectorMock.Verify(c => c.SendMessage(response));
			jobMock.VerifyAll();
		}

		private Mock<IWorkerConnector> SetupConnectorReceive<TRequest>(TRequest request)
			where TRequest : WorkMessageBase, new()
		{
			Action<TRequest> handler = null;
			var connectorMock = services.Mock<IWorkerConnector>();
			// capture the handler
			connectorMock.Setup(c => c.RegisterAsyncHandler(It.IsAny<Action<TRequest>>()))
				.Callback((Action<TRequest> h) => { handler = h; });

			// run the handler after entering consumer poller
			connectorMock.Setup(c => c.EnterConsumer()).Callback(() => { handler(request); });
			return connectorMock;
		}

		private void DoErrorStatusOnException<TRequest, TResponse>()
			where TRequest : WorkMessageBase, new() where TResponse : ReplyMessageBase, new()
		{
			var jobMock = services.Mock<IJobExecutor<TRequest, TResponse>>();
			var connectorMock = SetupConnectorReceive(new TRequest());

			// simulate an error
			jobMock.Setup(j => j.Execute(It.IsAny<TRequest>())).Throws<Exception>();

			GetWorker().Run();

			// check that a response was actually sent
			connectorMock.Verify(c => c.SendMessage(It.Is<TResponse>(r => r.Status == Status.Error)));
			jobMock.VerifyAll();
		}

		private void DoNeverSendsNull<TRequest, TResponse>()
			where TRequest : WorkMessageBase, new() where TResponse : ReplyMessageBase, new()
		{
			var jobMock = services.Mock<IJobExecutor<TRequest, TResponse>>();
			var connectorMock = SetupConnectorReceive(new TRequest());

			// simulate an error
			jobMock.Setup(j => j.Execute(It.IsAny<TRequest>())).Returns((TResponse) null);

			GetWorker().Run();

			// check that a response was actually sent
			connectorMock.Verify(c => c.SendMessage(It.Is<TResponse>(r => r.Status == Status.Error)));
			jobMock.VerifyAll();
		}

		[Fact]
		public void ErrorStatusOnException()
		{
			DoErrorStatusOnException<MatchExecutionRequest, MatchExecutionResult>();
			DoErrorStatusOnException<SubmissionValidationRequest, SubmissionValidationResult>();
		}

		[Fact]
		public void NeverSendsNull()
		{
			DoNeverSendsNull<MatchExecutionRequest, MatchExecutionResult>();
			DoNeverSendsNull<SubmissionValidationRequest, SubmissionValidationResult>();
		}

		[Fact]
		public void RespondsToMessages()
		{
			DoRespondsToMessages<MatchExecutionRequest, MatchExecutionResult>();
			DoRespondsToMessages<SubmissionValidationRequest, SubmissionValidationResult>();
		}

		[Fact]
		public void StartupTest()
		{
			var connectorMock = services.Mock<IWorkerConnector>();

			GetWorker().Run();

			connectorMock.Verify(c => c.SendMessage(It.IsAny<WorkerConnectMessage>()));
			connectorMock.Verify(c => c.EnterConsumer());
			connectorMock.Verify(c => c.EnterPoller());
		}
	}
}
