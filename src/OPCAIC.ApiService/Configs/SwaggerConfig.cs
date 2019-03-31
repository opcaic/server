using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Swashbuckle.AspNetCore.Swagger;
using Swashbuckle.AspNetCore.SwaggerGen;
using Swashbuckle.AspNetCore.SwaggerUI;

namespace OPCAIC.ApiService.Configs
{
	public class SwaggerConfig
	{
		private const string _DocumentName = "v1";

		private const string _DocumentRouteTemplate = "/swagger/{documentName}/swagger.json";
		private const string _DocumentEndpoint = "/swagger/v1/swagger.json";

		private const string _ApiVersion = "V1";
		private const string _Title = "OPCAIC API";

		public static void SetupSwaggerGen(SwaggerGenOptions options)
		{
			options.SwaggerDoc(_DocumentName, new Info
			{
				Version = _ApiVersion,
				Title = _Title
			});
			options.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory,
				$"{Assembly.GetEntryAssembly().GetName().Name}.xml"));
			options.AddSecurityDefinition("Bearer", new ApiKeyScheme
			{
				Description =
					"JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
				Name = "Authorization",
				In = "header",
				Type = "apiKey"
			});
			var security = new Dictionary<string, IEnumerable<string>>
			{
				{"Bearer", new string[] { }}
			};
			options.AddSecurityRequirement(security);
		}

		public static void SetupSwaggerUI(SwaggerUIOptions options)
		{
			options.SwaggerEndpoint(_DocumentEndpoint, _DocumentName);
		}

		public static void SetupSwagger(SwaggerOptions options)
		{
			options.RouteTemplate = _DocumentRouteTemplate;
		}
	}
}
