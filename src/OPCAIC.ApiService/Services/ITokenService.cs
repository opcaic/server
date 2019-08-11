using System;
using System.Collections.Generic;
using System.Security.Claims;

namespace OPCAIC.ApiService.Services
{
	public interface ITokenService
	{
		string CreateToken(string key, TimeSpan? expiresIn, params Claim[] claims);
		IEnumerable<Claim> ValidateToken(string key, string token);
	}
}