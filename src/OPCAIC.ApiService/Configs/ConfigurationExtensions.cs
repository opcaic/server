using Microsoft.Extensions.Configuration;

namespace OPCAIC.ApiService.Configs
{
	public static class ConfigurationExtensions
	{
		public static JwtConfiguration GetJwtConfiguration(
			this IConfiguration configuration)
		{
			var conf = new JwtConfiguration();
			configuration.Bind(ConfigNames.Jwt, conf);
			return conf;
		}

		public static string GetAppBaseUrl(this IConfiguration configuration)
		{
			return configuration.GetValue<string>(ConfigNames.AppBaseUrl);
		}

		public static string GetServerBaseUrl(this IConfiguration configuration)
		{
			return configuration.GetValue<string>(ConfigNames.ServerBaseUrl);
		}
	}
}