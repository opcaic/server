using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Moq;
using OPCAIC.Messaging;
using OPCAIC.Messaging.Config;
using OPCAIC.Messaging.Messages;
using OPCAIC.TestUtils;
using OPCAIC.Worker.GameModules;
using OPCAIC.Worker.Services;
using Xunit;
using Xunit.Abstractions;

namespace OPCAIC.Worker.Test
{
	internal class WorkerConnectorHelper
	{
		private readonly Dictionary<Type, Action<object>> asyncHandlers =
			new Dictionary<Type, Action<object>>();

		private Action consumerActions;
		private Action socketActions;
		private Dictionary<Type, Action<object>> syncHandlers = new Dictionary<Type, Action<object>>();

		public WorkerConnectorHelper(Mock<IWorkerConnector> mock)
		{
			Mock = mock;

			Mock.Setup(c => c.EnterConsumer()).Callback(PlayConsumer);
			Mock.Setup(c => c.EnterSocket()).Callback(PlaySocket);
		}

		public Mock<IWorkerConnector> Mock { get; }

		private void CallAsyncHandler<T>(T arg)
		{
			if (!asyncHandlers.TryGetValue(typeof(T), out var action))
			{
				throw new InvalidOperationException($"No handler for type {typeof(T)} captured.");
			}

			asyncHandlers[typeof(T)](arg);
		}

		private void CallHandler<T>(T arg)
		{
			if (!syncHandlers.TryGetValue(typeof(T), out var action))
			{
				throw new InvalidOperationException($"No handler for type {typeof(T)} captured.");
			}

			syncHandlers[typeof(T)](arg);
		}

		public void SetupConsumerReceive<TRequest>(TRequest request) where TRequest : WorkMessageBase
		{
			Mock.Setup(c => c.RegisterAsyncHandler(It.IsAny<Action<TRequest>>()))
				.Callback((Action<TRequest> h) => asyncHandlers[typeof(TRequest)] = o => h((TRequest) o));

			consumerActions += () => CallAsyncHandler(request);
		}

		public void SetupConsumer(Action action) => consumerActions += action;

		public void SetupSocket(Action action) => socketActions += action;

		public void SetupSocketReceive<TRequest>(TRequest request)
		{
			Mock.Setup(c => c.RegisterHandler(It.IsAny<Action<TRequest>>()))
				.Callback((Action<TRequest> h) => syncHandlers[typeof(TRequest)] = o => h((TRequest) o));

			socketActions += () => CallHandler(request);
		}

		private void PlayConsumer()
		{
			try
			{
				consumerActions?.Invoke();
			}
			catch 
			{
				// do nothing
			}
		}

		private void PlaySocket()
		{
			try
			{
				socketActions?.Invoke();
			}
			catch 
			{
				// do nothing
			}
		}
	}

	public class WorkerTest : ServiceTestBase
	{
		private readonly WorkerConnectorHelper connectorHelper;

		public WorkerTest(ITestOutputHelper output) : base(output)
		{
			Services
				.AddTransient<IGameModuleRegistry, GameModuleRegistry>()
				.AddSingleton<IOptions<ExecutionConfig>>(new OptionsWrapper<ExecutionConfig>(
					new ExecutionConfig
					{
						MaxTaskTimeout = 1000
					}));

			connectorHelper = new WorkerConnectorHelper(Services.Mock<IWorkerConnector>());
		}

		private void RunWorker() => GetService<Worker>().Run();

		[Theory]
		[MemberData(nameof(RequestTestData))]
		public void RespondsToMessages<TRequest, TResponse>(TRequest request, TResponse response)
			where TRequest : WorkMessageBase, new() where TResponse : ReplyMessageBase, new()
		{
			var jobMock = Services.Mock<IJobExecutor<TRequest, TResponse>>();
			connectorHelper.SetupConsumerReceive(request);

			// setup response from job executor
			jobMock.Setup(j => j.Execute(request, It.IsAny<CancellationToken>())).Returns(response);

			RunWorker();

			// check that the respons was actually sent
			connectorHelper.Mock.Verify(c => c.SendMessage(response));
			jobMock.VerifyAll();
		}

		[Theory]
		[MemberData(nameof(RequestTestData))]
		public void ErrorStatusOnException<TRequest, TResponse>(TRequest request, TResponse _)
			where TRequest : WorkMessageBase, new() where TResponse : ReplyMessageBase, new()
		{
			var jobMock = Services.Mock<IJobExecutor<TRequest, TResponse>>();
			connectorHelper.SetupConsumerReceive(request);

			// simulate an error
			jobMock.Setup(j => j.Execute(It.IsAny<TRequest>(), It.IsAny<CancellationToken>()))
				.Throws<Exception>();

			RunWorker();

			// check that a response was actually sent
			connectorHelper.Mock.Verify(c => c.SendMessage(It.Is<TResponse>(r => r.Status == Status.Error)));
			jobMock.VerifyAll();
		}

		[Theory]
		[MemberData(nameof(RequestTestData))]
		public void NeverSendsNull<TRequest, TResponse>(TRequest request, TResponse _)
			where TRequest : WorkMessageBase, new() where TResponse : ReplyMessageBase, new()
		{
			var jobMock = Services.Mock<IJobExecutor<TRequest, TResponse>>();
			connectorHelper.SetupConsumerReceive(request);

			// simulate an error
			jobMock.Setup(j => j.Execute(It.IsAny<TRequest>(), It.IsAny<CancellationToken>()))
				.Returns((TResponse) null);

			RunWorker();

			// check that a response was actually sent
			connectorHelper.Mock.Verify(c => c.SendMessage(It.Is<TResponse>(r => r.Status == Status.Error)));
			jobMock.VerifyAll();
		}

		public static TheoryData<WorkMessageBase, ReplyMessageBase> RequestTestData()
		{
			var ret = new TheoryData<WorkMessageBase, ReplyMessageBase>();
			ret.Add(new MatchExecutionRequest(), new MatchExecutionResult());
			ret.Add(new SubmissionValidationRequest(), new SubmissionValidationResult());

			return ret;
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
			connectorHelper.SetupSocketReceive(new WorkerResetMessage() { HeartbeatConfig = config });

			RunWorker();

			connectorHelper.Mock.Verify(c => c.SetHeartbeatConfig(config));
		}

		[Fact]
		public void CancelsRunningJob()
		{
			TimeSpan timeout = TimeSpan.FromMilliseconds(100);

			var request = new MatchExecutionRequest();
			var jobMock = Services.Mock<IJobExecutor<MatchExecutionRequest, MatchExecutionResult>>();
			ManualResetEventSlim workStarted = new ManualResetEventSlim(false);

			connectorHelper.SetupConsumerReceive(request);
			connectorHelper.SetupSocket(() => workStarted.Wait());
			connectorHelper.SetupSocketReceive(new WorkerResetMessage());

			jobMock.Setup(j => j.Execute(request, It.IsAny<CancellationToken>())).Callback(
				(MatchExecutionRequest req, CancellationToken token) =>
				{
					workStarted.Set();
					AssertEx.WaitForEvent(() => token.IsCancellationRequested, timeout);
				});

			RunWorker();

			connectorHelper.Mock.VerifyAll();
			jobMock.VerifyAll();
		}
	}
}
