namespace OPCAIC.ApiService.Models.Users
{
	public class UserTokens
	{
		/// <summary>
		/// new access token used to authorize access to resources (default expiration is 60 seconds)
		/// </summary>
		public string AccessToken { get; set; }

		/// <summary>
		/// new refresh token used to obtain new access token (default expiration is 7 days)
		/// </summary>
		public string RefreshToken { get; set; }
	}
}
