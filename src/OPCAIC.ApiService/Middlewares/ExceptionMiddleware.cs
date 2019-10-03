using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using OPCAIC.ApiService.Exceptions;
using OPCAIC.ApiService.Security;
using OPCAIC.ApiService.Utils;
using OPCAIC.Application.Exceptions;
using OPCAIC.Application.Infrastructure.Validation;
using OPCAIC.Common;
using OPCAIC.Utils;
using ConflictException = OPCAIC.Application.Exceptions.ConflictException;

namespace OPCAIC.ApiService.Middlewares
{
	public sealed class ExceptionMiddleware
	{
		private static readonly string logMessageTemplate =
			$"HTTP {{{LoggingTags.HttpRequestMethod}}} {{{LoggingTags.HttpRequestPath}}} responded {{{LoggingTags.HttpStatusCode}}} in {{{LoggingTags.HttpElapsedTime}:0.0000}} ms";

		private static readonly Action<ILogger, string, string, int, double, Exception> infoLog =
			LoggerMessage.Define<string, string, int, double>(LogLevel.Information, 0,
				logMessageTemplate);

		private static readonly Action<ILogger, string, string, int, double, Exception> errorLog =
			LoggerMessage.Define<string, string, int, double>(LogLevel.Error, 0,
				logMessageTemplate);

		private static readonly JsonSerializerSettings jsonSerializerSettings =
			new JsonSerializerSettings
			{
				ContractResolver = new DefaultContractResolver
				{
					NamingStrategy = new CamelCaseNamingStrategy()
				},
				Formatting = Formatting.Indented
			};

		private readonly IHostingEnvironment env;

		private readonly ILogger logger;

		private readonly RequestDelegate next;

		public ExceptionMiddleware(RequestDelegate next, IHostingEnvironment env,
			ILoggerFactory loggerFactory)
		{
			logger = loggerFactory.CreateLogger("RequestLogger");
			this.next = next;
			this.env = env;
		}

		public async Task InvokeAsync(HttpContext context)
		{
			Require.ArgNotNull(context, nameof(context));

			var start = Stopwatch.GetTimestamp();
			var scope = logger.BeginScope(GetAdditionalProperties(context));
			try
			{
				await next(context);
				Log(context, context.Response.StatusCode, GetElapsedMs(start), null);
			}
			// logging inside when() ensures that exception is logged before all logging scopes are
			// left, so their data can be seen in the logged event. The Log function always returns
			// true
			catch (ModelValidationException ex)
				when (Log(context, ex.StatusCode, GetElapsedMs(start), null))
			{
				await WriteResponseAsync(context, ex.StatusCode, ex.ValidationErrors.ToList());
			}
			catch (ApiException ex)
				when (Log(context, ex.StatusCode, GetElapsedMs(start), null))
			{
				await WriteResponseAsync(context, ex);
			}
			catch (ConflictException ex)
				when (Log(context, StatusCodes.Status409Conflict, GetElapsedMs(start), null))
			{
				await WriteResponseAsync(context, StatusCodes.Status409Conflict, ex.Error);
			}
			catch (NotFoundException ex)
				when (Log(context, StatusCodes.Status404NotFound, GetElapsedMs(start), null))
			{
				await WriteResponseAsync(context, StatusCodes.Status404NotFound,
					new {ex.ResourceId, ex.Resource, ex.Message});
			}
			catch (BusinessException ex)
				when (Log(context, StatusCodes.Status400BadRequest, GetElapsedMs(start), null))
			{
				await WriteResponseAsync(context, StatusCodes.Status400BadRequest, ex.Error);
			}
			catch (BadHttpRequestException ex)
				when (Log(context, ex.StatusCode, GetElapsedMs(start), null))
			{
				await WriteResponseAsync(context,
					new ApiException(ex.StatusCode, ex.Message, null));
			}
			catch (Exception ex)
				when (Log(context, 500, GetElapsedMs(start), ex) &&
					env.IsDevelopment())
			{
				if (env.IsDevelopment())
				{
					await WriteResponseAsync(context, 500, ex);
				}
				else
				{
					await WriteResponseAsync(context,
						new ApiException(500, "Internal server error", null));
				}
			}
		}

		private bool Log(HttpContext context, int statusCode, double elapsedMs, Exception ex)
		{
			if (statusCode >= 500)
			{
				errorLog(logger, context.Request.Method, context.Request.Path, statusCode,
					elapsedMs, ex);
			}
			else
			{
				infoLog(logger, context.Request.Method, context.Request.Path, statusCode,
					elapsedMs, ex);
			}

			return true;
		}

		private static double GetElapsedMs(long startTimestamp)
		{
			var stop = Stopwatch.GetTimestamp();
			return (stop - startTimestamp) * 1000.0 / Stopwatch.Frequency;
		}

		private static async Task WriteResponseAsync(HttpContext context, ApiException apiException)
		{
			var model = new {apiException.Message, apiException.Code};
			await WriteResponseAsync(context, apiException.StatusCode, model);
		}

		private static Task WriteResponseAsync(HttpContext context, int statusCode,
			ApplicationError error)
		{
			return WriteResponseAsync(context, statusCode, new List<ApplicationError> {error});
		}

		private static Task WriteResponseAsync(HttpContext context, int statusCode,
			List<ApplicationError> errors)
		{
			foreach (var error in errors.OfType<ValidationError>())
			{
				error.Field = error.Field.FirstLetterToLower();
			}

			return WriteResponseAsync(context, statusCode,
				new ValidationErrorModel {Errors = errors});
		}

		public static Task WriteResponseAsync(HttpContext context, int statusCode, object model)
		{
			context.Response.StatusCode = statusCode;
			var json = JsonConvert.SerializeObject(model, jsonSerializerSettings);

			context.Response.ContentType = "application/json";
			return context.Response.WriteAsync(json, context.RequestAborted);
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
	}
}