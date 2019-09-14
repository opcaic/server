using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using OPCAIC.ApiService.Services;
using OPCAIC.Domain.Entities;
using OPCAIC.Infrastructure.DbContexts;

namespace OPCAIC.ApiService.Security
{
	internal static class IdentitySetup
	{
		public static void ConfigureIdentity(this IServiceCollection services)
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
		}

	}
}