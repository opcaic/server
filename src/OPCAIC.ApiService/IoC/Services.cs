using Microsoft.Extensions.DependencyInjection;
using OPCAIC.ApiService.ModelValidationHandling;
using OPCAIC.ApiService.Services;
using OPCAIC.Infrastructure.Emails;
using OPCAIC.Services;

namespace OPCAIC.ApiService.IoC
{
	public static class Services
	{
		public static void AddServices(this IServiceCollection services)
		{
			services
				.AddTransient<IEmailService, EmailService>()
				.AddTransient<IUserService, UserService>()
				.AddTransient<IStorageService, StorageService>()
				.AddTransient<ITokenService, TokenService>()
				.AddTransient<IGamesService, GamesService>()
				.AddTransient<ITournamentsService, TournamentsService>()
				.AddTransient<IDocumentService, DocumentService>()
				.AddScoped<IModelValidationService, ModelValidationService>();
		}
	}
}