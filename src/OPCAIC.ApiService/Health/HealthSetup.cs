using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Newtonsoft.Json;
using OPCAIC.Infrastructure.DbContexts;

namespace OPCAIC.ApiService.Health
{
	internal static class HealthSetup
	{
		public static readonly HealthCheckOptions Options = new HealthCheckOptions
		{
			ResponseWriter = ResponseWriter
		};

		public static void ConfigureHealth(this IServiceCollection services)
		{
			services
				.AddHealthChecks()
				.AddDbContextCheck<DataContext>()
				.AddCheck<BrokerHealthCheck>("broker");
		}

		private static Task ResponseWriter(HttpContext context, HealthReport report)
		{
			return context.Response.WriteAsync(JsonConvert.SerializeObject(report));
		}
	}
}