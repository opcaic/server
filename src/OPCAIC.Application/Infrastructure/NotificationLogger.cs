using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Extensions.Logging;
using OPCAIC.Common;

namespace OPCAIC.Application.Infrastructure
{
	public class NotificationLogger<TNotification> : INotificationHandler<TNotification>
		where TNotification : INotification
	{
		private static readonly Action<ILogger, TNotification, Exception> logAction =
			LoggerMessage.Define<TNotification>(LogLevel.Information, 0,
				$"{typeof(TNotification).Name} {{@{LoggingTags.Payload}}}");

		private readonly ILogger<TNotification> logger;

		public NotificationLogger(ILogger<TNotification> logger)
		{
			this.logger = logger;
		}

		/// <inheritdoc />
		public Task Handle(TNotification notification, CancellationToken cancellationToken)
		{
			logAction(logger, notification, null);
			return Task.CompletedTask;
		}
	}
}