using Microsoft.Extensions.Configuration;

namespace OPCAIC.ApiService.Configs
{
	public static class ConfigurationExtensions
	{
		public static SecurityConfiguration GetSecurityConfiguration(this IConfiguration configuration)
		{
			var conf = configuration.GetSection(ConfigNames.Security);
			return new SecurityConfiguration
			{
				Key = conf.GetValue<string>(nameof(SecurityConfiguration.Key)),
				AccessTokenExpirationSeconds = conf.GetValue<int>(nameof(SecurityConfiguration.AccessTokenExpirationSeconds)),
				RefreshTokenExpirationDays = conf.GetValue<int>(nameof(SecurityConfiguration.RefreshTokenExpirationDays))
			};
		}
	}
}
