using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Extensions.Logging;
using OPCAIC.Application.Exceptions;
using OPCAIC.Application.Logging;
using OPCAIC.Common;

namespace OPCAIC.ApiService.Behaviors
{
	public class RequestLoggingBehavior<TRequest, TResponse>
		: IPipelineBehavior<TRequest, TResponse>
	{
		private static readonly string messageTemplate =
			$"{typeof(TRequest).Name} {{@{LoggingTags.Payload}}}";

		private static readonly Action<ILogger, double, TRequest, Exception> slowRequestLogAction =
			LoggerMessage.Define<double, TRequest>(LogLevel.Warning, LoggingEvents.RequestTooLong,
				$"Slow request ({{{LoggingTags.HttpElapsedTime}:0.000}} ms) {messageTemplate}");

		private static readonly Action<ILogger, TRequest, Exception> logAction =
			LoggerMessage.Define<TRequest>(LogLevel.Information, 0, messageTemplate);

		private static readonly Action<ILogger, TRequest, Exception> logActionError =
			LoggerMessage.Define<TRequest>(LogLevel.Error, 0, messageTemplate);

		private readonly ILogger<TRequest> logger;

		public RequestLoggingBehavior(ILogger<TRequest> logger)
		{
			this.logger = logger;
		}

		/// <inheritdoc />
		public async Task<TResponse> Handle(TRequest request, CancellationToken cancellationToken,
			RequestHandlerDelegate<TResponse> next)
		{
			using var scope = logger.CreateScopeWithIds(request);
			try
			{
				logAction(logger, request, null);

				var sw = Stopwatch.StartNew();

				var response = await next();

				if (sw.ElapsedMilliseconds > 100)
				{
					slowRequestLogAction(logger, sw.ElapsedMilliseconds, request, null);
				}

				return response;
			}
			catch (Exception e) when (!(e is AppException))
			{
				logActionError(logger, request, e);
				throw;
			}
		}
	}
}