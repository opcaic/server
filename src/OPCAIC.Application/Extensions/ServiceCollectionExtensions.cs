using Microsoft.Extensions.DependencyInjection;
using OPCAIC.Application.Interfaces;
using OPCAIC.Application.Services;

namespace OPCAIC.Application.Extensions
{
	public static class ServiceCollectionExtensions
	{
		public static IServiceCollection AddMatchGenerators(this IServiceCollection services)
		{
			return services
				.AddSingleton<IOngoingMatchGenerator, EloMatchGenerator>()
				.AddSingleton<IDeadlineMatchGenerator, SinglePlayerMatchGenerator>()
				.AddSingleton<IDeadlineMatchGenerator, TableMatchGenerator>()
				.AddSingleton<IBracketsMatchGenerator, SingleEliminationMatchGenerator>()
				.AddSingleton<IBracketsMatchGenerator, DoubleEliminationMatchGenerator>()
				.AddSingleton<IMatchTreeFactory, CachedMatchTreeFactory>()
				.AddSingleton<IMatchGenerator, MatchGeneratorRegistry>();
		}
	}
}