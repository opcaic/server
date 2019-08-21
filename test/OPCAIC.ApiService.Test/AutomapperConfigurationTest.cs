using Xunit;

namespace OPCAIC.ApiService.Test
{
	public class AutomapperConfigurationTest
	{
		[Fact]
		public void ConfigurationCorrect()
		{
			MapperConfigurationFactory.Create().AssertConfigurationIsValid();
		}
	}
}