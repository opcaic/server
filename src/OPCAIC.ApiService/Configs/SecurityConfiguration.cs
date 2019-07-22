namespace OPCAIC.ApiService.Configs
{
	public class SecurityConfiguration
	{
		public string Key { get; set; }

		public int AccessTokenExpirationSeconds { get; set; }

		public int RefreshTokenExpirationDays { get; set; }
	}
}
