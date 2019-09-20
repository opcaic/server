using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.Logging;
using OPCAIC.ApiService.Exceptions;
using OPCAIC.ApiService.Security;
using OPCAIC.Common;
using OPCAIC.Utils;

namespace OPCAIC.ApiService.Middlewares
{
	public class LoggingMiddleware
	{
		private readonly ILogger<LoggingMiddleware> logger;
		private readonly RequestDelegate next;
		private bool logged;

		public LoggingMiddleware(RequestDelegate next, ILogger<LoggingMiddleware> logger)
		{
			this.next = next;
			this.logger = logger;
		}

		public async Task InvokeAsync(HttpContext context)
		{
			Require.ArgNotNull(context, nameof(context));

			var start = Stopwatch.GetTimestamp();

			var scope = logger.BeginScope(GetAdditionalProperties(context));
			logged = false;
			try
			{
				await next(context);
				var elapsedMs = GetElapsedMilliseconds(start);
				var statusCode = context.Response.StatusCode;
				DoLog(context, statusCode, elapsedMs, null);
			}
			// following catch blocks are never reached because DoLog returns false, logging inside
			// when() ensures that exception is logged before all logging scopes are left, so their
			// data can be seen in the logged event.
			catch (ApiException ex) when (DoLog(context, ex.StatusCode,
				GetElapsedMilliseconds(start), null))
			{
			}
			catch (BadHttpRequestException ex) when (DoLog(context, ex.StatusCode, GetElapsedMilliseconds(start), null))
			{
			}
			catch (Exception ex) when (DoLog(context, 500, GetElapsedMilliseconds(start), ex))
			{
			}
			finally
			{
				scope.Dispose();
			}
		}

		private Dictionary<string, object> GetAdditionalProperties(HttpContext context)
		{
			var props = new Dictionary<string, object>();

			var user = context.User;
			if (user.Identity.IsAuthenticated)
			{
				props.Add(LoggingTags.UserId, user.FindFirstValue(ClaimTypes.NameIdentifier));
				props.Add(LoggingTags.UserName, user.FindFirstValue(ClaimTypes.Name));
				props.Add(LoggingTags.UserEmail, user.FindFirstValue(ClaimTypes.Email));
				props.Add(LoggingTags.UserRole, user.FindFirstValue(RolePolicy.UserRoleClaim));
			}

			return props;
		}

		private bool DoLog(HttpContext context, int statusCode, double elapsedMs, Exception ex)
		{
			var level = statusCode >= 500 ? LogLevel.Error : LogLevel.Information;

			if (!logger.IsEnabled(level) || logged)
			{
				return false;
			}

			const string logMessageTemplate = "HTTP {" +
				LoggingTags.HttpRequestMethod +
				"} {" +
				LoggingTags.HttpRequestPath +
				"} responded {" +
				LoggingTags.HttpStatusCode +
				"} in {" +
				LoggingTags.HttpElapsedTime +
				":0.0000} ms";
			logger.Log(level, 0, ex, logMessageTemplate, context.Request.Method,
				context.Request.Path, statusCode, elapsedMs);
			logged = true;

			return false;
		}

		private double GetElapsedMilliseconds(long startTimestamp)
		{
			var stop = Stopwatch.GetTimestamp();
			return (stop - startTimestamp) * 1000.0 / Stopwatch.Frequency;
		}
	}
}