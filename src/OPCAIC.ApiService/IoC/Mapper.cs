using AutoMapper;
using Microsoft.Extensions.DependencyInjection;
using OPCAIC.Application.Matches;

namespace OPCAIC.ApiService.IoC
{
	public static class AutoMapper
	{
		public static void AddMapper(this IServiceCollection services)
		{
			services.AddAutoMapper(typeof(Startup).Assembly, typeof(MatchMapperProfile).Assembly);
		}
	}
}