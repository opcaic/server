using Microsoft.Extensions.DependencyInjection;
using OPCAIC.ApiService.Services;
using OPCAIC.Application.Interfaces.Repositories;
using OPCAIC.Persistence.Repositories;
using OPCAIC.Persistence.Repositories.Emails;

namespace OPCAIC.ApiService.IoC
{
	public static class Repositories
	{
		public static void AddRepositories(this IServiceCollection serviceCollection)
		{
			serviceCollection
				.AddTransient<IEmailRepository, EmailRepository>()
				.AddTransient<IEmailTemplateRepository, EmailTemplateRepository>()
				.AddTransient<IUserRepository, UserRepository>()
				.AddTransient<IMatchRepository, MatchRepository>()
				.AddTransient<ITournamentRepository, TournamentRepository>()
				.AddTransient<ITournamentInvitationRepository, TournamentInvitationRepository>()
				.AddTransient<ITournamentParticipationsRepository, TournamentParticipationsRepository>()
				.AddTransient<ISubmissionRepository, SubmissionRepository>()
				.AddTransient<ISubmissionValidationRepository, SubmissionValidationRepository>()
				.AddTransient<IMatchExecutionRepository, MatchExecutionRepository>()
				.AddTransient<IUserTournamentRepository, UserTournamentRepository>()
				.AddTransient<IGameRepository, GameRepository>()
				.AddTransient<IDocumentRepository, DocumentRepository>();
		}
	}
}