using System;
using System.Security.Claims;

namespace OPCAIC.ApiService.Interfaces
{
	public interface IJwtTokenService
	{
		ClaimsPrincipal ExtractClaimsPrincipal(string token);
		string CreateToken(TimeSpan? expiresIn, ClaimsIdentity identity);
	}
}