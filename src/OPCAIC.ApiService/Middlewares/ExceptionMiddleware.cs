using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using OPCAIC.ApiService.Exceptions;
using OPCAIC.ApiService.Utils;
using OPCAIC.Application.Exceptions;
using OPCAIC.Application.Infrastructure.Validation;

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
				await WriteResponseAsync(context, ex.StatusCode, ex.ValidationErrors.ToList());
			}
			catch (ApiException ex)
			{
				await WriteResponseAsync(context, ex);
			}
			catch (NotFoundException ex)
			{
				await WriteResponseAsync(context, StatusCodes.Status404NotFound,
					new {ex.ResourceId, ex.Resource, ex.Message});
			}
			catch (BusinessException ex)
			{
				await WriteResponseAsync(context, StatusCodes.Status400BadRequest, ex.Error);
			}
			catch (BadHttpRequestException ex)
			{
				await WriteResponseAsync(context,
					new ApiException(ex.StatusCode, ex.Message, null));
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
	}
}