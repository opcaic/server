using Microsoft.Extensions.DependencyInjection;
using OPCAIC.ApiService.ModelValidationHandling;
using OPCAIC.ApiService.Services;
using OPCAIC.Infrastructure.Emails;
using OPCAIC.Services;
using OPCAIC.Services.Extensions;

namespace OPCAIC.ApiService.IoC
{
	public static class Services
	{
		public static void AddServices(this IServiceCollection services)
		{
			services
				.AddMatchGenerators()
				.AddHostedService<BrokerReactor>()
				.AddHostedService<TournamentProcessor>()
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
				.AddScoped<IFrontendUrlGenerator, UrlGenerator>()
				.AddScoped<IWorkerUrlGenerator, UrlGenerator>()
				.AddSingleton<IModelValidationService, ModelValidationService>()
				.AddScoped<IMatchService, MatchService>()
				.AddScoped<ISubmissionValidationService, SubmissionValidationService>()
				.AddScoped<IMatchExecutionService, MatchExecutionService>()
				.AddScoped<IWorkerService, WorkerService>()
				.AddScoped<IBrokerService, BrokerService>()
				.AddScoped<ILeaderboardService, LeaderboardService>()
				.AddScoped<IMatchTreeFactory, CachedMatchTreeFactory>();
		}
	}
}