using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
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

			options.OperationFilter<HybridOperationFilter>();
			options.SchemaFilter<InterfaceMemberFilter>(typeof(IIdentifiedRequest));
			options.SchemaFilter<InterfaceMemberFilter>(typeof(IAuthenticatedRequest));
			options.SchemaFilter<InterfaceMemberFilter>(typeof(IPublicRequest));
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

	public class HybridOperationFilter : IOperationFilter
	{
		public void Apply(Operation operation, OperationFilterContext context)
		{
			var hybridParameters = context.ApiDescription.ParameterDescriptions
				.Where(x => x.Source.Id == "Hybrid")
				.Select(x => new
				{
					name = x.Name,
					schema = context.SchemaRegistry.GetOrRegister(x.Type)
				}).ToList();

			for (var i = 0; i < operation.Parameters.Count; i++)
			{
				for (var j = 0; j < hybridParameters.Count; j++)
				{
					if (hybridParameters[j].name == operation.Parameters[i].Name)
					{
						var name = operation.Parameters[i].Name;
						var isRequired = operation.Parameters[i].Required;

						operation.Parameters.RemoveAt(i);

						operation.Parameters.Insert(i, new BodyParameter()
						{
							Name = name,
							Required = isRequired,
							Schema = hybridParameters[j].schema,
						});
					}
				}
			}
		}
	}
}