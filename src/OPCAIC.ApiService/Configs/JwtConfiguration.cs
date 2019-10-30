namespace OPCAIC.ApiService.Configs
{
	public class JwtConfiguration
	{
		public string Key { get; set; }

		public int AccessTokenExpirationMinutes { get; set; }

		public int RefreshTokenExpirationDays { get; set; }

		public int WorkerTokenExpirationMinutes { get; set; }
	}
}