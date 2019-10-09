using Microsoft.Extensions.DependencyInjection;
using OPCAIC.Application.Interfaces.Repositories;
using OPCAIC.Application.Specifications;
using OPCAIC.Persistence.Repositories;

namespace OPCAIC.ApiService.IoC
{
	public static class Repositories
	{
		public static void AddRepositories(this IServiceCollection serviceCollection)
		{
			serviceCollection
				.AddScoped<IUserRepository, UserRepository>()
				.AddScoped<IMatchRepository, MatchRepository>()
				.AddScoped<ITournamentRepository, TournamentRepository>()
				.AddScoped<ISubmissionRepository, SubmissionRepository>()
				.AddScoped<ISubmissionValidationRepository, SubmissionValidationRepository>()
				.AddScoped<IMatchExecutionRepository, MatchExecutionRepository>()
				.AddScoped<IDocumentRepository, DocumentRepository>()
				.AddScoped(typeof(IRepository<>), typeof(Repository<>));
		}
	}
}