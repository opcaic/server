using Microsoft.Extensions.DependencyInjection;
using OPCAIC.ApiService.ModelValidationHandling;
using OPCAIC.ApiService.Services;
using OPCAIC.Infrastructure.Emails;
using OPCAIC.Infrastructure.Identity;
using OPCAIC.Services;

namespace OPCAIC.ApiService.IoC
{
	public static class Services
	{
		public static void AddServices(this IServiceCollection services)
		{
			services
				.AddTransient<IEmailService, EmailService>()
				.AddTransient<IUserManager, UserManager>()
				.AddTransient<IStorageService, StorageService>()
				.AddTransient<IGamesService, GamesService>()
				.AddTransient<ITournamentsService, TournamentsService>()
				.AddTransient<ITournamentParticipantsService, TournamentParticipantsService>()
				.AddTransient<EmailSender>()
				.AddHostedService<EmailCronService>()
				.AddTransient<IDocumentService, DocumentService>()
				.AddTransient<IFrontendUrlGenerator, FrontendUrlGenerator>()
				.AddScoped<IModelValidationService, ModelValidationService>()
				.AddTransient<IMatchService, MatchService>();
		}
	}
}