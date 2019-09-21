using AutoMapper;
using Microsoft.Extensions.DependencyInjection;
using OPCAIC.ApiService.IoC;
using OPCAIC.ApiService.Test.Services;
using Xunit;

namespace OPCAIC.ApiService.Test
{
	public class AutomapperConfigurationTest
	{
		[Fact]
		public void ConfigurationCorrect()
		{
			TestMapper.Mapper.ConfigurationProvider.AssertConfigurationIsValid();
		}
	}
}