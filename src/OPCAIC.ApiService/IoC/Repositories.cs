using Microsoft.Extensions.DependencyInjection;
using OPCAIC.Infrastructure.Repositories;
using OPCAIC.Infrastructure.Repositories.Emails;

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
        .AddTransient<ISubmissionRepository, SubmissionRepository>()
        .AddTransient<IUserTournamentRepository, UserTournamentRepository>()
        .AddTransient<IGameRepository, GameRepository>();
    }
  }
}