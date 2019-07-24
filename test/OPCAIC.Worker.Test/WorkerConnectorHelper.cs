using System;
using System.Collections.Generic;
using Moq;
using OPCAIC.Messaging;
using OPCAIC.Messaging.Messages;

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
}