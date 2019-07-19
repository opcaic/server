using Microsoft.Extensions.DependencyInjection;
using OPCAIC.Services;

namespace OPCAIC.Infrastructure.Extensions
{
	public static class ServiceCollectionExtensions
	{
		public static IServiceCollection AddMatchGenerators(this IServiceCollection services)
		{
			return services
				.AddSingleton<IMatchGenerator, SinglePlayerMatchGenerator>()
				.AddSingleton<IMatchGenerator, TableMatchGenerator>()
				.AddTransient<IMatchGenerator, SingleEliminationMatchGenerator>()
				.AddTransient<IMatchGenerator, DoubleEliminationMatchGenerator>()
				.AddSingleton<IMatchTreeFactory, CachedMatchTreeFactory>();
		}
	}
}
