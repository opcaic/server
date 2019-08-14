using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using OPCAIC.ApiService.Exceptions;
using OPCAIC.ApiService.ModelValidationHandling;

namespace OPCAIC.ApiService.Middlewares
{
	public sealed class ExceptionMiddleware
	{
		private readonly RequestDelegate next;

		public ExceptionMiddleware(RequestDelegate next)
		{
			this.next = next;
		}

		public async Task InvokeAsync(HttpContext context)
		{
			if (context == null)
			{
				throw new ArgumentNullException(nameof(context));
			}

			try
			{
				await next(context);
			}
			catch (ModelValidationException ex)
			{
				await WriteResponseAsync(context, ex);
			}
			catch (ApiException ex)
			{
				await WriteResponseAsync(context, ex);
			}
			catch (Exception ex)
			{
				await WriteResponseAsync(context, new ApiException(500, ex.Message));
			}
		}

		private static async Task WriteResponseAsync(HttpContext context, ApiException apiException)
		{
			context.Response.StatusCode = apiException.StatusCode;

			if (apiException.Message != null)
			{
				var model = new {apiException.Message};

				var json = JsonConvert.SerializeObject(model);

				context.Response.ContentType = "application/json";
				await context.Response.WriteAsync(json, context.RequestAborted);
			}
		}

		private static async Task WriteResponseAsync(HttpContext context,
			ModelValidationException modelValidationException)
		{
			context.Response.StatusCode = modelValidationException.StatusCode;

			var model = new
			{
				Errors = new List<ValidationErrorBase> {modelValidationException.ValidationError},
				Title = "Invalid arguments to the API", // TODO: do not duplicate code
				Detail = "The inputs supplied to the API are invalid" // TODO: do not duplicate code
			};

			var json = JsonConvert.SerializeObject(model);

			context.Response.ContentType = "application/json";
			await context.Response.WriteAsync(json, context.RequestAborted);
		}
	}
}