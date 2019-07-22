using AutoMapper;
using Microsoft.Extensions.DependencyInjection;

namespace OPCAIC.ApiService.IoC
{
	public static class AutoMapper
	{
		public static void AddMapper(this IServiceCollection services)
		{
			services.AddSingleton(MapperConfigurationFactory.Create());
			services.AddTransient<IMapper>(provider
				=> new Mapper(provider.GetService<MapperConfiguration>()));
		}
	}
}