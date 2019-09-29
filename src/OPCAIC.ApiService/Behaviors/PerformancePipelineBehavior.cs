using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Extensions.Logging;
using OPCAIC.Common;

namespace OPCAIC.ApiService.Behaviors
{
	public class PerformancePipelineBehavior<TRequest, TResponse>
		: IPipelineBehavior<TRequest, TResponse>
	{
		private static Action<ILogger, TRequest, double, Exception> logAction =
			LoggerMessage.Define<TRequest, double>(LogLevel.Warning, LoggingEvents.RequestTooLong,
				$"Request {{@{LoggingTags.Payload}}} took {{{LoggingTags.HttpElapsedTime}:0.000}} ms");

		private readonly ILogger<TRequest> logger;

		public PerformancePipelineBehavior(ILogger<TRequest> logger)
		{
			this.logger = logger;
		}

		/// <inheritdoc />
		public async Task<TResponse> Handle(TRequest request, CancellationToken cancellationToken,
			RequestHandlerDelegate<TResponse> next)
		{
			var sw = Stopwatch.StartNew();

			var response = await next();

			if (sw.ElapsedMilliseconds > 100)
			{
				logAction(logger, request, sw.ElapsedMilliseconds, null);
			}

			return response;
		}
	}
}