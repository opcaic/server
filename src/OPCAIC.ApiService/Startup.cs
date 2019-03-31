using System;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using OPCAIC.ApiService.Configs;
using OPCAIC.ApiService.IoC;

namespace OPCAIC
{
	public class Startup
	{
		public Startup(IConfiguration configuration) => Configuration = configuration;

		public IConfiguration Configuration { get; }

		// This method gets called by the runtime. Use this method to add services to the container.
		public void ConfigureServices(IServiceCollection services)
		{
			services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_2);
			var key = Encoding.ASCII.GetBytes(
				Environment.GetEnvironmentVariable(EnvVariables._SecurityKey));

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

			services.AddServices();
			services.AddSwaggerGen(SwaggerConfig.SetupSwaggerGen);
		}

		public void Configure(IApplicationBuilder app, IHostingEnvironment env)
		{
			app.UseHsts();

			app.UseAuthentication();

			app.UseSwagger(SwaggerConfig.SetupSwagger);
			app.UseSwaggerUI(SwaggerConfig.SetupSwaggerUI);

			app.UseHttpsRedirection();
			app.UseMvc();
		}
	}
}
