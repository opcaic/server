using System;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using OPCAIC.ApiService.Configs;
using OPCAIC.ApiService.IoC;
using OPCAIC.ApiService.Middlewares;
using OPCAIC.ApiService.ModelValidationHandling;
using OPCAIC.ApiService.Security;
using OPCAIC.ApiService.Services;
using OPCAIC.Broker;
using OPCAIC.Infrastructure.DbContexts;
using OPCAIC.Infrastructure.Emails;
using OPCAIC.Infrastructure.Entities;
using OPCAIC.Infrastructure.Identity;
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

		public void ConfigureSecurity(IServiceCollection services)
		{
			services
				.AddIdentity<User, Role>(options =>
				{
					// use lax settings for now
					options.Password.RequireDigit = false;
					options.Password.RequireLowercase = false;
					options.Password.RequireNonAlphanumeric = false;
					options.Password.RequireUppercase = false;
					options.Password.RequiredLength = 4;
					options.Password.RequiredUniqueChars = 1;

					options.User.RequireUniqueEmail = true;

					options.SignIn.RequireConfirmedEmail = true;
				})
				.AddUserManager<UserManager>()
				.AddEntityFrameworkStores<DataContext>()
				.AddErrorDescriber<AppIdentityErrorDescriber>()
				.AddDefaultTokenProviders()
				.AddTokenProvider<JwtTokenProvider>(nameof(JwtTokenProvider));

			services.AddScoped<SignInManager>();

			var conf = Configuration.GetSecurityConfiguration();

			var signingKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(conf.Key));

			services.Configure<SecurityConfiguration>(
				Configuration.GetSection(ConfigNames.Security));

			services.Configure<JwtIssuerOptions>(cfg =>
			{
				// TODO: Issuer and Audience from config?
				cfg.SigningCredentials =
					new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256Signature);
			});

			services.AddAuthentication(x =>
				{
					x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
					x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
				})
				.AddJwtBearer(x =>
				{
					x.RequireHttpsMetadata = false;
					x.SaveToken = false;
					x.TokenValidationParameters = new TokenValidationParameters
					{
						ValidateIssuerSigningKey = true,
						IssuerSigningKey = signingKey,
						ValidateIssuer = false,
						ValidateAudience = false,
						ClockSkew = TimeSpan.Zero
					};
				});

			services.AddAuthorization(AuthorizationConfiguration.Setup);
		}

		public void ConfigureServices(IServiceCollection services)
		{
			services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_2)
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
				});

			ConfigureSecurity(services);

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

			services.AddServices();
			services.AddBroker();
			services.AddRepositories();
			services.AddMapper();
			services.AddSwaggerGen(SwaggerConfig.SetupSwaggerGen);

			ConfigureOptions(services);
		}

		public void ConfigureOptions(IServiceCollection services)
		{
			services.Configure<UrlGeneratorConfiguration>(Configuration);
			services.Configure<StorageConfiguration>(Configuration.GetSection("Storage"));
			services.Configure<EmailsConfiguration>(Configuration.GetSection("Emails"));
			services.Configure<SecurityConfiguration>(Configuration.GetSection("Security"));
			services.Configure<BrokerConnectorConfig>(Configuration.GetSection("Broker"));
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

			app.UseAuthentication();

			app.UseMiddleware<ExceptionMiddleware>();
			app.UseMiddleware<LoggingMiddleware>();
			app.UseMiddleware<DbTransactionMiddleware>();

			app.UseMvc();
		}
	}
}