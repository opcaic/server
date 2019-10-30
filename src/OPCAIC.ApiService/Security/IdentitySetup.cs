using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OPCAIC.ApiService.Services;
using OPCAIC.Domain.Entities;
using OPCAIC.Persistence;

namespace OPCAIC.ApiService.Security
{
	internal static class IdentitySetup
	{
		public static void ConfigureIdentity(this IServiceCollection services,
			IConfiguration configuration, ILogger logger)
		{
			services
				.AddIdentity<User, Role>(options =>
				{
					// use lax settings for now
					options.Password.RequireDigit = false;
					options.Password.RequireLowercase = false;
					options.Password.RequireNonAlphanumeric = false;
					options.Password.RequireUppercase = false;
					options.Password.RequiredUniqueChars = 1;

					// allow overwrite by config values
					configuration.GetSection("Security:Password").Bind(options.Password);
					if (options.Password.RequiredLength < 8)
					{
						logger.LogWarning("Passwords must have at least 8 characters, setting RequiredLength to 8");
						// always require at least 8 characters
						options.Password.RequiredLength = 8;
					}

					options.User.RequireUniqueEmail = true;

					options.SignIn.RequireConfirmedEmail = true;
				})
				.AddUserManager<UserManager>()
				.AddEntityFrameworkStores<DataContext>()
				.AddErrorDescriber<AppIdentityErrorDescriber>()
				.AddDefaultTokenProviders()
				.AddTokenProvider<JwtTokenProvider>(nameof(JwtTokenProvider));

			services.AddScoped<SignInManager>();
		}

	}
}