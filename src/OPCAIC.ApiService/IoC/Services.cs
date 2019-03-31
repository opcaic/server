using Microsoft.Extensions.DependencyInjection;
using OPCAIC.ApiService.Services;

namespace OPCAIC.ApiService.IoC
{
	public static class Services
	{
		public static void AddServices(this IServiceCollection services)
		{
			services.AddTransient<IUserService, UserService>();
		}
	}
}
