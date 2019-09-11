using AutoMapper;

namespace OPCAIC.ApiService.Test.Services
{
	internal static class TestMapper
	{
		// Static IMapper instance to be used via `using static TestMapper`
		public static readonly IMapper Mapper = new Mapper(MapperConfigurationFactory.Create());
	}
}