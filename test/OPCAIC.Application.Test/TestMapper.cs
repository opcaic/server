using AutoMapper;
using Microsoft.Extensions.DependencyInjection;
using OPCAIC.ApiService;
using OPCAIC.ApiService.IoC;
using OPCAIC.Application.Infrastructure.AutoMapper;

namespace OPCAIC.Application.Test
{
	public static class TestMapper
	{
		// Static IMapper instance to be used via `using static TestMapper`
		public static readonly IMapper Mapper = ConstructMapper();

		private static IMapper ConstructMapper()
		{
			return new Mapper(new MapperConfiguration(cfg =>
			{
				cfg.AddProfile<AutomapperProfile>();
				cfg.AddProfile(new AutoMapperProfile(typeof(AutoMapperProfile).Assembly));
			}));
		}
	}
}