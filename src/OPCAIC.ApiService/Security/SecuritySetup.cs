﻿using System;
using System.Text;
using AspNetCoreRateLimit;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using OPCAIC.ApiService.Configs;
using OPCAIC.ApiService.Interfaces;
using OPCAIC.ApiService.Security.Handlers;
using OPCAIC.ApiService.Services;

namespace OPCAIC.ApiService.Security
{
	internal static class SecuritySetup
	{
		public static void ConfigureApiThrottling(this IServiceCollection services)
		{
			services
				.AddSingleton<IIpPolicyStore, MemoryCacheIpPolicyStore>()
				.AddSingleton<IRateLimitCounterStore, MemoryCacheRateLimitCounterStore>()
				.AddSingleton<IRateLimitConfiguration, RateLimitConfiguration>();
		}

		public static void ConfigureSecurity(this IServiceCollection services, IConfiguration configuration, ILogger logger)
		{
			services.ConfigureApiThrottling();
			services.ConfigureIdentity(configuration, logger);

			services
				.AddSingleton<IAuthorizationHandler, SuperUserAuthorizationHandler>()
				.AddScoped<IAuthorizationHandler, UserPermissionHandler>()
				.AddScoped<IAuthorizationHandler, EmailPermissionHandler>()
				.AddScoped<IAuthorizationHandler, TournamentPermissionHandler>()
				.AddScoped<IAuthorizationHandler, DocumentPermissionHandler>()
				.AddScoped<IAuthorizationHandler, SubmissionPermissionHandler>()
				.AddScoped<IAuthorizationHandler, MatchExecutionPermissionHandler>()
				.AddScoped<IAuthorizationHandler, SubmissionValidationPermissionHandler>()
				.AddScoped<IAuthorizationHandler, MatchPermissionHandler>()
				.AddScoped<IAuthorizationHandler, GamePermissionHandler>();

			services.AddSingleton<IJwtTokenService, JwtTokenService>();

			var conf = configuration.GetJwtConfiguration();

			var signingKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(conf.Key));

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

	}
}