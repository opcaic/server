using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using OPCAIC.ApiService.Exceptions;
using OPCAIC.ApiService.Utils;

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
			catch (Exception ex)
			{
				if (env.IsDevelopment())
				{
					await WriteResponseAsync(context, new ApiException(500, ex.Message, null));
				}
				else
				{
					await WriteResponseAsync(context,
						new ApiException(500, "Internal server error", null));
				}
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

			var model = new
			{
				Errors = modelValidationException.ValidationErrors.ToList(),
				Title = "Invalid arguments to the API", // TODO: do not duplicate code
				Detail = "The inputs supplied to the API are invalid" // TODO: do not duplicate code
			};

			await WriteResponseAsync(context, modelValidationException.StatusCode, model);
		}

		private static Task WriteResponseAsync(HttpContext context, int statusCode, object model)
		{
			context.Response.StatusCode = statusCode;
			var json = JsonConvert.SerializeObject(model, jsonSerializerSettings);

			context.Response.ContentType = "application/json";
			return context.Response.WriteAsync(json, context.RequestAborted);
		}
	}
}