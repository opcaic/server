using Microsoft.Extensions.Configuration;

namespace OPCAIC.ApiService.Configs
{
	public static class ConfigurationExtensions
	{
		public static SecurityConfiguration GetSecurityConfiguration(
			this IConfiguration configuration)
		{
			var conf = new SecurityConfiguration();
			configuration.Bind(ConfigNames.Security, conf);
			return conf;
		}
	}
}