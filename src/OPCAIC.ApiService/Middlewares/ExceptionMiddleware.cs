using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using OPCAIC.ApiService.Exceptions;
using OPCAIC.ApiService.Utils;
using OPCAIC.Domain.Exceptions;

namespace OPCAIC.ApiService.Middlewares
{
	public sealed class ExceptionMiddleware
	{
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

		private readonly RequestDelegate next;

		public ExceptionMiddleware(RequestDelegate next, IHostingEnvironment env)
		{
			this.next = next;
			this.env = env;
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
			catch (NotFoundException ex)
			{
				await WriteResponseAsync(context, StatusCodes.Status404NotFound, ex);
			}
			catch(BadHttpRequestException ex)
			{
				await WriteResponseAsync(context, new ApiException(ex.StatusCode, ex.Message, null));
			}
			catch (Exception ex) when (env.IsDevelopment())
			{
				await WriteResponseAsync(context, 500, ex);
			}
			catch
			{
				await WriteResponseAsync(context,
					new ApiException(500, "Internal server error", null));
			}
		}

		private static async Task WriteResponseAsync(HttpContext context, ApiException apiException)
		{
			var model = new {apiException.Message, apiException.Code};
			await WriteResponseAsync(context, apiException.StatusCode, model);
		}

		private static async Task WriteResponseAsync(HttpContext context,
			ModelValidationException modelValidationException)
		{
			foreach (var error in modelValidationException.ValidationErrors)
			{
				error.Field = error.Field.FirstLetterToLower();
			}

			await WriteResponseAsync(context, modelValidationException.StatusCode, new ValidationErrorModel
			{
				Errors = modelValidationException.ValidationErrors.ToList(),
			});
		}

		public static Task WriteResponseAsync(HttpContext context, int statusCode, object model)
		{
			context.Response.StatusCode = statusCode;
			var json = JsonConvert.SerializeObject(model, jsonSerializerSettings);

			context.Response.ContentType = "application/json";
			return context.Response.WriteAsync(json, context.RequestAborted);
		}
	}
}