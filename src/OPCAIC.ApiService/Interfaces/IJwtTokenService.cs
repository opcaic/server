using System;
using System.Security.Claims;

namespace OPCAIC.ApiService.Services
{
	public interface IJwtTokenService
	{
		ClaimsPrincipal ExtractClaimsPrincipal(string token);
		string CreateToken(TimeSpan? expiresIn, ClaimsIdentity identity);
	}
}