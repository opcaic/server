using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using OPCAIC.ApiService.Interfaces;
using OPCAIC.ApiService.Security;

namespace OPCAIC.ApiService.Services
{
	public class JwtTokenService : IJwtTokenService
	{
		private readonly JwtIssuerOptions jwtOptions;
		private readonly JwtSecurityTokenHandler tokenHandler = new JwtSecurityTokenHandler();

		public JwtTokenService(IOptions<JwtIssuerOptions> jwtOptions)
		{
			this.jwtOptions = jwtOptions.Value;
		}

		private TokenValidationParameters GetValidationParameters()
		{
			return new TokenValidationParameters
			{
				ValidateLifetime = true,
				ValidateAudience = false,
				ValidateIssuer = false,
				IssuerSigningKey = jwtOptions.SigningCredentials.Key
			};
		}


		public ClaimsPrincipal ExtractClaimsPrincipal(string token)
		{
			try
			{
				return tokenHandler.ValidateToken(token, GetValidationParameters(), out _);
			}
			catch (ArgumentException)
			{
				// malformed token
				return null;
			}
		}

		public string CreateToken(TimeSpan? expiresIn, ClaimsIdentity identity)
		{
			var tokenDescriptor = new SecurityTokenDescriptor
			{
				Subject = identity,
				Expires =
					expiresIn != null ? DateTime.Now.Add(expiresIn.Value) : (DateTime?)null,
				Audience = jwtOptions.Audience,
				Issuer = jwtOptions.Issuer,
				SigningCredentials = jwtOptions.SigningCredentials
			};
			var token = tokenHandler.CreateToken(tokenDescriptor);

			return tokenHandler.WriteToken(token);
		}

	}
}