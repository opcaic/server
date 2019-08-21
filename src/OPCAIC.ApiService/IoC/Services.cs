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
				.AddScoped<IEmailService, EmailService>()
				.AddScoped<IUserManager, UserManager>()
				.AddScoped<IStorageService, StorageService>()
				.AddScoped<IGamesService, GamesService>()
				.AddScoped<ITournamentsService, TournamentsService>()
				.AddScoped<ITournamentParticipantsService, TournamentParticipantsService>()
				.AddTransient<EmailSender>()
				.AddHostedService<EmailCronService>()
				.AddScoped<IDocumentService, DocumentService>()
				.AddScoped<ISubmissionService, SubmissionService>()
				.AddScoped<IFrontendUrlGenerator, FrontendUrlGenerator>()
				.AddSingleton<IModelValidationService, ModelValidationService>()
				.AddScoped<IMatchService, MatchService>();
		}
	}
}