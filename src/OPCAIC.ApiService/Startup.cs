using FluentValidation.AspNetCore;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OPCAIC.ApiService.Configs;
using OPCAIC.ApiService.Health;
using OPCAIC.ApiService.IoC;
using OPCAIC.ApiService.Middlewares;
using OPCAIC.ApiService.ModelValidationHandling;
using OPCAIC.ApiService.Security;
using OPCAIC.Broker;
using OPCAIC.Infrastructure.DbContexts;
using OPCAIC.Infrastructure.Emails;
using OPCAIC.Messaging.Config;
using OPCAIC.Services;

[assembly: ApiController]

namespace OPCAIC.ApiService
{
	public class Startup
	{
		private readonly string myAllowSpecificOrigins = "_myAllowSpecificOrigins";

		public Startup(IConfiguration configuration)
		{
			Configuration = configuration;
		}

		public IConfiguration Configuration { get; }

		public void ConfigureServices(IServiceCollection services)
		{
			services.AddMvc()
				.SetCompatibilityVersion(CompatibilityVersion.Version_2_2)
				.ConfigureApiBehaviorOptions(options =>
				{
					options.SuppressInferBindingSourcesForParameters = true;
					options.InvalidModelStateResponseFactory = context =>
					{
						var apiErrorService = context.HttpContext.RequestServices
							.GetRequiredService<IModelValidationService>();
						var problems = new CustomBadRequest(context, apiErrorService);
						return new BadRequestObjectResult(problems);
					};
				}).AddFluentValidation(options =>
				{
					options.RegisterValidatorsFromAssemblyContaining<Startup>();
				});
			services.AddTransient<IValidatorInterceptor, ValidationInterceptor>();

			services.AddCors(options =>
			{
				options.AddPolicy(myAllowSpecificOrigins,
					builder =>
					{
						builder.AllowAnyOrigin()
							.AllowCredentials()
							.AllowAnyHeader()
							.AllowAnyMethod();
					});
			});

			// TODO: replace with real database
			services.AddDbContext<DataContext>(options => options.UseInMemoryDatabase("Dummy"));

			services.AddMediatR(typeof(Startup).Assembly);
			services.AddServices();
			services.AddBroker();
			services.AddRepositories();
			services.AddMapper();
			services.AddSwaggerGen(SwaggerConfig.SetupSwaggerGen);

			services.ConfigureSecurity(Configuration);
			services.ConfigureHealth();
			ConfigureOptions(services);
		}

		public void ConfigureOptions(IServiceCollection services)
		{
			services.Configure<UrlGeneratorConfiguration>(Configuration);
			services.Configure<SecurityConfiguration>(
				Configuration.GetSection(ConfigNames.Security));
			services.Configure<StorageConfiguration>(Configuration.GetSection("Storage"));
			services.Configure<EmailsConfiguration>(Configuration.GetSection("Emails"));
			services.Configure<SecurityConfiguration>(Configuration.GetSection("Security"));
			services.Configure<BrokerConnectorConfig>(Configuration.GetSection("Broker"));
			services.Configure<BrokerOptions>(Configuration.GetSection("Broker"));
		}

		public void Configure(IApplicationBuilder app, IHostingEnvironment env)
		{
			if (env.IsDevelopment())
			{
				app.UseDeveloperExceptionPage();
				app.UseCors(myAllowSpecificOrigins);
			}
			else
			{
				app.UseHsts();
				app.UseHttpsRedirection();
			}

			app.UseSwagger(SwaggerConfig.SetupSwagger);
			app.UseSwaggerUI(SwaggerConfig.SetupSwaggerUi);
			app.UseHealthChecks("/api/health", HealthSetup.Options);

			app.UseAuthentication();

			app.UseMiddleware<ExceptionMiddleware>();
			app.UseMiddleware<LoggingMiddleware>();
			app.UseMiddleware<DbTransactionMiddleware>();

			app.UseMvc();
		}
	}
}