using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using OPCAIC.ApiService.Interfaces;
using OPCAIC.ApiService.Security;
using OPCAIC.Common;

namespace OPCAIC.ApiService.Services
{
	public class JwtTokenService : IJwtTokenService
	{
		private readonly ITimeService time;
		private readonly JwtIssuerOptions jwtOptions;
		private readonly JwtSecurityTokenHandler tokenHandler = new JwtSecurityTokenHandler();

		public JwtTokenService(IOptions<JwtIssuerOptions> jwtOptions, ITimeService time)
		{
			this.time = time;
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
					expiresIn != null ? time.Now.Add(expiresIn.Value) : (DateTime?)null,
				Audience = jwtOptions.Audience,
				Issuer = jwtOptions.Issuer,
				SigningCredentials = jwtOptions.SigningCredentials
			};
			var token = tokenHandler.CreateToken(tokenDescriptor);

			return tokenHandler.WriteToken(token);
		}

	}
}