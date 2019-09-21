using AutoMapper;
using Microsoft.Extensions.DependencyInjection;
using OPCAIC.ApiService.IoC;

namespace OPCAIC.ApiService.Test.Services
{
	internal static class TestMapper
	{
		// Static IMapper instance to be used via `using static TestMapper`
		public static readonly IMapper Mapper = ConstructMapper();

		private static IMapper ConstructMapper()
		{
			var services = new ServiceCollection();
			services.AddMapper();
			return services.BuildServiceProvider().GetRequiredService<IMapper>();
		}
	}
}