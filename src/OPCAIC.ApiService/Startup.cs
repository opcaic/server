﻿using FluentValidation.AspNetCore;
using MediatR;
using MediatR.Pipeline;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using OPCAIC.ApiService.Behaviors;
using OPCAIC.ApiService.Configs;
using OPCAIC.ApiService.Health;
using OPCAIC.ApiService.IoC;
using OPCAIC.ApiService.Middlewares;
using OPCAIC.ApiService.ModelBinding;
using OPCAIC.ApiService.ModelValidationHandling;
using OPCAIC.ApiService.Security;
using OPCAIC.ApiService.Serialization;
using OPCAIC.Application.Infrastructure;
using OPCAIC.Application.Services;
using OPCAIC.Application.Tournaments.Events;
using OPCAIC.Broker;
using OPCAIC.Infrastructure.Emails;
using OPCAIC.Messaging.Config;
using OPCAIC.Persistence;
using Serilog;

[assembly: ApiController]

namespace OPCAIC.ApiService
{
	public class Startup
	{
		private readonly string myAllowSpecificOrigins = "_myAllowSpecificOrigins";

		public Startup(IConfiguration configuration, IWebHostEnvironment environment)
		{
			Configuration = configuration;
			Environment = environment;
		}

		public IConfiguration Configuration { get; }
		public IWebHostEnvironment Environment { get; }

		public void ConfigureServices(IServiceCollection services)
		{
			services.AddControllers(options =>
				{
					options.ModelMetadataDetailsProviders.Add(new ExcludeInterfaceMetadataProvider(typeof(IPublicRequest)));
					options.ModelMetadataDetailsProviders.Add(new ExcludeInterfaceMetadataProvider(typeof(IAuthenticatedRequest)));
				})
				.ConfigureJsonOptions()
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
					options.RegisterValidatorsFromAssemblyContaining<TournamentFinished>();
				});
			services.AddTransient<IValidatorInterceptor, ValidationInterceptor>();

			services.AddCors(options =>
			{
				options.AddPolicy(myAllowSpecificOrigins,
					builder =>
					{
						builder.AllowAnyOrigin()
							.AllowAnyHeader()
							.AllowAnyMethod();
					});
			});

			var connectionString = Configuration.GetConnectionString(nameof(DataContext));
			if (Environment.IsDevelopment() && connectionString == null)
			{
				var connection = new SqliteConnection("DataSource=:memory:");
				connection.Open();
				// allow in-memory db in development
				services.AddDbContext<DataContext>(options => options.UseSqlite(connection));
			}
			else
			{
				services.AddDbContext<DataContext>(options => options.UseNpgsql(connectionString));
			}

			services.AddMediatR(typeof(Startup).Assembly, typeof(TournamentFinished).Assembly);
			services.AddSingleton(typeof(IRequestPreProcessor<>),
				typeof(UserRequestPreprocessor<>));
			services.AddSingleton(typeof(IPipelineBehavior<,>),
				typeof(PerformancePipelineBehavior<,>));

			services.AddServices();
			services.AddBroker();
			services.AddRepositories();
			services.AddMapper();
			services.AddMemoryCache();
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
			services.Configure<RequestSizeConfig>(Configuration.GetSection("Limits"));
			services.Configure<StorageConfiguration>(Configuration.GetSection("Storage"));
			services.Configure<EmailsConfiguration>(Configuration.GetSection("Emails"));
			services.Configure<SecurityConfiguration>(Configuration.GetSection("Security"));
			services.Configure<BrokerConnectorConfig>(Configuration.GetSection("Broker"));
			services.Configure<BrokerOptions>(Configuration.GetSection("Broker"));
		}

		public void Configure(IApplicationBuilder app)
		{
			app.UseRouting();

			if (Environment.IsDevelopment())
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

			app.UseAuthentication();
			app.UseAuthorization();

			app.UseMiddleware<ExceptionMiddleware>();
			app.UseMiddleware<DbTransactionMiddleware>();

			app.UseEndpoints(endpoints =>
			{
				endpoints.MapHealthChecks("/api/health", HealthSetup.Options);
				endpoints.MapControllers();
			});
		}
	}
}