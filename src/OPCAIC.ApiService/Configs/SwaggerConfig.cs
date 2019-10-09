using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.DependencyInjection;
using OPCAIC.ApiService.ModelBinding;
using OPCAIC.ApiService.Utils;
using OPCAIC.Application.Infrastructure;
using OPCAIC.Utils;
using Swashbuckle.AspNetCore.Swagger;
using Swashbuckle.AspNetCore.SwaggerGen;
using Swashbuckle.AspNetCore.SwaggerUI;

namespace OPCAIC.ApiService.Configs
{
	public class SwaggerConfig
	{
		private const string DocumentName = "v1";

		private const string DocumentRouteTemplate = "/swagger/{documentName}/swagger.json";
		private const string DocumentEndpoint = "/swagger/v1/swagger.json";

		private const string ApiVersion = "V1";
		private const string Title = "OPCAIC API";

		public static void SetupSwaggerGen(SwaggerGenOptions options)
		{
			options.SwaggerDoc(DocumentName, new Info { Version = ApiVersion, Title = Title });
			options.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory,
				$"{typeof(SwaggerConfig).Assembly.GetName().Name}.xml"));
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

			options.SchemaFilter<InterfaceMemberFilter>(typeof(IIdentifiedRequest));
			options.SchemaFilter<InterfaceMemberFilter>(typeof(IAuthenticatedRequest));
			options.SchemaFilter<InterfaceMemberFilter>(typeof(IPublicRequest));
			options.OperationFilter<RouteParamFilter>();
		}

		public static void SetupSwaggerUi(SwaggerUIOptions options)
		{
			options.SwaggerEndpoint(DocumentEndpoint, DocumentName);
		}

		public static void SetupSwagger(SwaggerOptions options)
		{
			options.RouteTemplate = DocumentRouteTemplate;
		}
	}

	public class InterfaceMemberFilter : ISchemaFilter
	{
		private readonly Type interfaceType;

		public InterfaceMemberFilter(Type interfaceType)
		{
			Require.ArgNotNull(interfaceType, nameof(interfaceType));
			Require.That<ArgumentException>(interfaceType.IsInterface, "Type is not interface.");

			this.interfaceType = interfaceType;
		}

		/// <inheritdoc />
		public void Apply(Schema schema, SchemaFilterContext context)
		{
			if (interfaceType.IsAssignableFrom(context.SystemType))
			{
				foreach (var property in interfaceType.GetProperties())
				{
					schema.Properties.Remove(property.Name.FirstLetterToLower());
				}
			}
		}
	}

	public class RouteParamFilter : IOperationFilter
	{
		/// <inheritdoc />
		public void Apply(Operation operation, OperationFilterContext context)
		{
			foreach (var param in context.ApiDescription.ParameterDescriptions
				.Where(p => p.Source == FromRouteAndBodyAttribute.RouteAndBodyBindingSource))
			{
				var schema = context.SchemaRegistry.Definitions[param.Type.Name];

				int idx = operation.Parameters.IndexOf(operation.Parameters.First(p => p.Name == param.Name));

				foreach (var pathParam in context.ApiDescription.ParameterDescriptions.Where(p => p.Source == BindingSource.Path))
				{
					schema.Properties.Remove(pathParam.Name);
				}

				operation.Parameters.RemoveAt(idx);
				operation.Parameters.Insert(idx, new BodyParameter
				{
					Name = param.Name,
					Required = true,
					Schema = schema
				});
			}
		}
	}
}