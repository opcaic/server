using Microsoft.IdentityModel.Tokens;

namespace OPCAIC.ApiService.Security
{
	public class JwtIssuerOptions
	{
		public string Issuer { get; set; }
		public string Audience { get; set; }
		public SigningCredentials SigningCredentials { get; set; }
	}
}