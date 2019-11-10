using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Extensions.Logging;
using OPCAIC.Application.Logging;
using OPCAIC.Utils;

namespace OPCAIC.Application.Infrastructure
{
	public class Mediator : IMediator
	{
		private readonly ServiceFactory serviceFactory;
		private readonly MediatR.Mediator innerMediator;

		public Mediator(ServiceFactory serviceFactory)
		{
			this.serviceFactory = serviceFactory;
			innerMediator = new MediatR.Mediator(serviceFactory);
		}

		/// <inheritdoc />
		public async Task<TResponse> Send<TResponse>(IRequest<TResponse> request,
			CancellationToken cancellationToken = new CancellationToken())
		{
			return await innerMediator.Send(request, cancellationToken);
		}

		/// <inheritdoc />
		public async Task Publish(object notification, CancellationToken cancellationToken = new CancellationToken())
		{
			Require.ArgNotNull(notification, nameof(notification));

			var logger = ((ILoggerFactory) serviceFactory(typeof(ILoggerFactory))).CreateLogger(notification
				.GetType().Name);

			using var scope = logger.CreateScopeWithIds(notification);

			await innerMediator.Publish(notification, cancellationToken);
		}

		/// <inheritdoc />
		public async Task Publish<TNotification>(TNotification notification,
			CancellationToken cancellationToken = new CancellationToken()) where TNotification : INotification
		{
			Require.ArgNotNull(notification, nameof(notification));

			var logger = ((ILogger<TNotification>) serviceFactory(typeof(ILogger<TNotification>)));

			using var scope = logger.CreateScopeWithIds(notification);

			await innerMediator.Publish(notification, cancellationToken);
		}

	}
}