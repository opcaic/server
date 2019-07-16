using OPCAIC.Broker;

namespace OPCAIC.ApiService
{
	using System;
	using System.Text;
	using Configs;
	using Infrastructure.DbContexts;
	using IoC;
	using Microsoft.AspNetCore.Authentication.JwtBearer;
	using Microsoft.AspNetCore.Builder;
	using Microsoft.AspNetCore.Hosting;
	using Microsoft.AspNetCore.Mvc;
	using Microsoft.EntityFrameworkCore;
	using Microsoft.Extensions.Configuration;
	using Microsoft.Extensions.DependencyInjection;
	using Microsoft.IdentityModel.Tokens;

	public class Startup
	{
		private readonly string myAllowSpecificOrigins = "_myAllowSpecificOrigins";

		public Startup(IConfiguration configuration) => Configuration = configuration;

		public IConfiguration Configuration { get; }

		// This method gets called by the runtime. Use this method to add services to the container.
		public void ConfigureServices(IServiceCollection services)
		{
			services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_2);
			var key = Encoding.ASCII.GetBytes(
				Environment.GetEnvironmentVariable(EnvVariables.SecurityKey));

			// Frontend app sources
			services.AddSpaStaticFiles(config =>
			{
				config.RootPath = Configuration.GetValue<string>("SPA_ROOT") ?? "wwwroot";
			});

			services.AddAuthentication(x =>
				{
					x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
					x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
				})
				.AddJwtBearer(x =>
				{
					x.RequireHttpsMetadata = false;
					x.SaveToken = true;
					x.TokenValidationParameters = new TokenValidationParameters
					{
						ValidateIssuerSigningKey = true,
						IssuerSigningKey = new SymmetricSecurityKey(key),
						ValidateIssuer = false,
						ValidateAudience = false
					};
				});

			services.AddCors(options =>
			{
				options.AddPolicy(myAllowSpecificOrigins,
					builder =>
					{
						builder.
							AllowAnyOrigin()
							.AllowCredentials()
							.AllowAnyHeader()
							.AllowAnyMethod();
					});
			});

			// TODO: replace with real database
			services.AddDbContext<EntityFrameworkDbContext>(options => options.UseInMemoryDatabase(databaseName: "Dummy"));

			services.AddServices();
			services.AddBroker(broker => Configuration.Bind("Broker", broker));
			services.AddSwaggerGen(SwaggerConfig.SetupSwaggerGen);
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

			app.UseAuthentication();

			app.UseSwagger(SwaggerConfig.SetupSwagger);
			app.UseSwaggerUI(SwaggerConfig.SetupSwaggerUi);

			// Serve the frontend SPA
			app.UseStaticFiles();
			app.UseDefaultFiles();
			app.UseSpaStaticFiles();

			app.UseMvc();
			app.UseSpa(spa => { });
		}
	}
}
