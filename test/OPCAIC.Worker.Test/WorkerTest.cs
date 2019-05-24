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

		private void RespondsToRequest<TRequest, TResponse>()
			where TRequest : WorkMessageBase, new() where TResponse : ReplyMessageBase, new()
		{
			var connectorMock = services.Mock<IWorkerConnector>();
			var jobMock = services.Mock<IJobExecutor<TRequest, TResponse>>();
			var request = new TRequest();
			var response = new TResponse();
			Action<TRequest> handler = null;

			// capture the handler
			connectorMock.Setup(c => c.RegisterAsyncHandler(It.IsAny<Action<TRequest>>()))
				.Callback((Action<TRequest> h) => { handler = h; });

			// run the handler after entering consumer poller
			connectorMock.Setup(c => c.EnterConsumer()).Callback(() => { handler(request); });

			// setup response from job executor
			jobMock.Setup(j => j.Execute(request)).Returns(response);

			GetWorker().Run();

			// check that the respons was actually sent
			connectorMock.Verify(c => c.SendMessage(response));
		}

		[Fact]
		public void RespondsToMessages()
		{
			RespondsToRequest<MatchExecutionRequest, MatchExecutionResult>();
			RespondsToRequest<SubmissionValidationRequest, SubmissionValidationResult>();
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
